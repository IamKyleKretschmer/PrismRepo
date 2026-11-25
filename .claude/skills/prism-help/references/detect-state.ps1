#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deterministically estimates the current Prism feature SDD state (see States.md).

.DESCRIPTION
    Walks the feature lifecycle states defined in States.md in order (01 -> 15), testing
    file- and git-based evidence for each. Tracks the highest contiguously-confirmed state
    ("latest confirmed state") plus any later states with partial evidence of having started
    ("unconfirmed states"), even though the confirmed streak broke before reaching them.

    Several state transitions in States.md rely on guards this script cannot verify at all
    (e.g. human sign-off, an agent's read-only analysis pass, work items in an external
    system). Each such state is called out with a NOTE comment beside its check function and
    returns Unconfirmable = $true; the confirmation loop then treats the following state's
    confirmation as proxy evidence that the unconfirmable one passed too, rather than letting
    it permanently cap the confirmed streak.

    When no such proxy is available, the confirmed streak stops at the gate and the gate is
    reported in blockedByUnconfirmableState, so callers can ask a human to adjudicate it
    without needing to inspect this script's source.

.PARAMETER FeatureDirectory
    Optional. A feature directory to scan directly, skipping Resolve-FeatureDir (the
    .specify/feature.json pin and branch-name matching). Relative paths are resolved against
    the repo root. If the path doesn't exist, falls back to the same behavior as if no feature
    directory could be resolved.

.OUTPUTS
    JSON: {
      featureDirectory,
      latestConfirmedState,
      latestUnconfirmedState,
      unconfirmedStates: [ { id, summary } ],
      blockedByUnconfirmableState: { id, summary } | null
    }

    blockedByUnconfirmableState is non-null only when the confirmed streak stopped at a state
    this script cannot verify and no later state confirmed as proxy for it. It is the state a
    human must adjudicate before the reported state can advance.
#>

[CmdletBinding()]
param(
    # Optional feature directory to scan. Bypasses Resolve-FeatureDir when supplied.
    [string]$FeatureDirectory
)

$ErrorActionPreference = 'Stop'

$Placeholder = 'NEEDS CLARIFICATION'
$WorkItemRefPattern = 'AB#\d+|_workitems/edit/\d+|github\.com/[^)\s]+/issues/\d+'
$ChecklistItemPattern = '^\s*-\s*\[[ xX]\]'
$UnresolvedChecklistItemPattern = '^\s*-\s*\[\s\]'

# Task items are scoped to lines carrying a T### id, both in the current index format
# ("- [ ] T001 [P] Title -> tasks/T001-slug.md") and in the older inline tasks.md. Without the
# id requirement the template's Definition of Ready / Definition of Done checklists would be
# counted as tasks, which never tick and so would permanently cap progress at "in progress".
$TaskItemPattern = '^\s*-\s*\[[ xX]\]\s*T\d{3,}'
$TickedTaskItemPattern = '^\s*-\s*\[[xX]\]\s*T\d{3,}'

# ============================================================================
# Repo / branch / feature-directory discovery
# ============================================================================

# From the working directory, never this script's location: on the plugin route the script runs
# from the plugin, outside the project.
function Get-RepoRoot {
    $dir = (Get-Location).Path
    while ($dir) {
        if (Test-Path -LiteralPath (Join-Path $dir '.specify') -PathType Container) { return $dir }
        $parent = Split-Path $dir -Parent
        if ([string]::IsNullOrEmpty($parent) -or $parent -eq $dir) { break }
        $dir = $parent
    }
    try {
        $top = git rev-parse --show-toplevel 2>$null
        if ($LASTEXITCODE -eq 0 -and $top) { return $top }
    } catch {}
    return (Get-Location).Path
}

function Get-DefaultBranch {
    param([string]$RepoRoot)
    try {
        $ref = git -C $RepoRoot symbolic-ref refs/remotes/origin/HEAD 2>$null
        if ($LASTEXITCODE -eq 0 -and $ref) { return ($ref -replace '^refs/remotes/origin/', '') }
    } catch {}
    try {
        $cfg = git -C $RepoRoot config --get init.defaultBranch 2>$null
        if ($LASTEXITCODE -eq 0 -and $cfg) { return $cfg }
    } catch {}
    return 'main'
}

function Get-CurrentBranch {
    param([string]$RepoRoot)
    try {
        $branch = git -C $RepoRoot branch --show-current 2>$null
        if ($LASTEXITCODE -eq 0 -and $branch) { return $branch }
    } catch {}
    return $null
}

# Resolves the active feature directory. Priority: .specify/feature.json pin (authoritative,
# written by the specify phase), then a branch-name match under specs/ covering both the current
# "<type>/<feature>" / "<type>/<phase>-<feature>" formats from .prism/config/branch-naming.yml and
# the classic Spec Kit numeric/timestamp prefixes.
# NOTE: the configured formats themselves are not parsed here - a team using a format beyond the
# shapes handled below relies on the feature.json pin written by the specify phase.
function Resolve-FeatureDir {
    param([string]$RepoRoot, [string]$CurrentBranch)

    $featureJsonPath = Join-Path $RepoRoot '.specify/feature.json'
    if (Test-Path -LiteralPath $featureJsonPath -PathType Leaf) {
        try {
            $cfg = Get-Content -LiteralPath $featureJsonPath -Raw | ConvertFrom-Json
            if ($cfg.feature_directory) {
                $fd = $cfg.feature_directory
                if (-not [System.IO.Path]::IsPathRooted($fd)) { $fd = Join-Path $RepoRoot $fd }
                if (Test-Path -LiteralPath $fd -PathType Container) { return $fd }
            }
        } catch {}
    }

    if (-not $CurrentBranch) { return $null }
    $specsDir = Join-Path $RepoRoot 'specs'
    if (-not (Test-Path -LiteralPath $specsDir -PathType Container)) { return $null }

    $branchName = $CurrentBranch -replace '^[^/]+/', ''
    $prefix = $null
    if ($branchName -match '^(\d{8}-\d{6})-') { $prefix = $Matches[1] }
    elseif ($branchName -match '^(\d{3,})-') { $prefix = $Matches[1] }

    if ($prefix) {
        $found = @(Get-ChildItem -LiteralPath $specsDir -Filter "$prefix-*" -Directory -ErrorAction SilentlyContinue)
        if ($found.Count -eq 1) { return $found[0].FullName }
    }

    # Candidates in order: the branch minus its <type> segment, then minus a leading <phase>
    # segment too, since featureFolderFormat is normally just "<feature>" while the branch may
    # carry <type> and <phase> around it.
    $candidates = @($branchName)
    if ($branchName -match '^(spec|plan|tasks|impl)[-/](.+)$') { $candidates += $Matches[2] }

    foreach ($candidate in $candidates) {
        $direct = Join-Path $specsDir $candidate
        if (Test-Path -LiteralPath $direct -PathType Container) { return $direct }
    }
    return $null
}

# ============================================================================
# Small deterministic evidence helpers
# ============================================================================

function Test-FileContainsText {
    param([string]$Path, [string]$Text)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    return [bool](Select-String -LiteralPath $Path -Pattern $Text -SimpleMatch -Quiet -ErrorAction SilentlyContinue)
}

function Test-FileMatchesPattern {
    param([string]$Path, [string]$Pattern)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    return [bool](Select-String -LiteralPath $Path -Pattern $Pattern -Quiet -ErrorAction SilentlyContinue)
}

function Test-DirHasFiles {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { return $false }
    return [bool](Get-ChildItem -LiteralPath $Path -File -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1)
}

# Evidence of work in the code repo(s): does HEAD differ from the default branch outside specs/?
# Meaningless (and skipped) while sitting on the default branch itself.
function Test-CodeDiffOutsideSpecs {
    param([string]$RepoRoot, [string]$DefaultBranch, [bool]$IsDefaultBranch)
    if ($IsDefaultBranch) { return $false }
    try {
        $files = git -C $RepoRoot diff --name-only "$DefaultBranch...HEAD" 2>$null
        if ($LASTEXITCODE -ne 0) { return $false }
        return [bool]($files | Where-Object { $_ -notmatch '^specs/' } | Select-Object -First 1)
    } catch { return $false }
}

# ============================================================================
# Per-state checks (each independently reviewable; matches the numbering in States.md)
# ============================================================================

# 02: Spec in Progress -> confirmed once spec.md exists (spec drafting has started).
function Test-State02 {
    param([string]$SpecMd)
    $exists = Test-Path -LiteralPath $SpecMd -PathType Leaf
    [pscustomobject]@{ Confirmed = $exists; Evidence = $exists }
}

# 03: Spec Drafted. Ready for Quality Checklist -> confirmed once spec.md is complete: drafted
# with no outstanding placeholders.
function Test-State03 {
    param([string]$SpecMd)
    $exists = Test-Path -LiteralPath $SpecMd -PathType Leaf
    $confirmed = $exists -and -not (Test-FileContainsText -Path $SpecMd -Text $Placeholder)
    [pscustomobject]@{ Confirmed = $confirmed; Evidence = $exists }
}

# 04: Spec Checked. Ready for Grilling -> confirmed once checklists/requirements.md exists with
# every item resolved (no unticked boxes remain). /prism-specify writes and validates this
# checklist itself; on the deprecated /speckit-specify route /speckit-checklist produces it.
# NOTE: the PM/UX/EM/AO + tester review gate itself is not machine-checkable. A fully-resolved
# checklist is used as the deterministic proxy that the review concluded.
function Test-State04 {
    param([string]$ChecklistsDir, [string]$RequirementsMd)
    $requirementsExists = Test-Path -LiteralPath $RequirementsMd -PathType Leaf
    $hasItems = $requirementsExists -and (Test-FileMatchesPattern -Path $RequirementsMd -Pattern $ChecklistItemPattern)
    $hasUnresolvedItems = $requirementsExists -and (Test-FileMatchesPattern -Path $RequirementsMd -Pattern $UnresolvedChecklistItemPattern)
    [pscustomobject]@{
        Confirmed = $hasItems -and -not $hasUnresolvedItems
        Evidence  = $requirementsExists -or (Test-Path -LiteralPath $ChecklistsDir -PathType Container)
    }
}

# 05: Spec Complete. Ready for Technical Plan -> confirmed once grilling output exists: one or
# more spec-{role}.md companion files (spec.{aspect}.md on older features) and/or a populated
# clarifications/ folder.
function Test-State05 {
    param([string]$FeatureDir, [string]$ClarificationsDir)
    $companionFiles = @(
        Get-ChildItem -LiteralPath $FeatureDir -Filter 'spec-*.md' -File -ErrorAction SilentlyContinue
        Get-ChildItem -LiteralPath $FeatureDir -Filter 'spec.*.md' -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -ne 'spec.md' }
    )
    $hasClarificationsDir = Test-Path -LiteralPath $ClarificationsDir -PathType Container
    $clarificationsPopulated = $hasClarificationsDir -and (Test-DirHasFiles -Path $ClarificationsDir)
    [pscustomobject]@{
        Confirmed = $clarificationsPopulated -or ($companionFiles.Count -gt 0)
        Evidence  = $hasClarificationsDir -or ($companionFiles.Count -gt 0)
    }
}

# 06: Technical Plan in Progress -> confirmed once plan.md exists (planning has started).
function Test-State06 {
    param([string]$PlanMd)
    $exists = Test-Path -LiteralPath $PlanMd -PathType Leaf
    [pscustomobject]@{ Confirmed = $exists; Evidence = $exists }
}

# 07: Technical Plan Complete. Ready for Tasks -> confirmed once plan.md and its companion
# artefacts exist, plan.md carries its required sections, and no placeholders remain.
# NOTE: contracts/ is deliberately NOT required - /prism-plan omits it entirely when the feature
# introduces no external boundary, so its absence is not evidence of an unfinished plan.
# NOTE: the whole-team plan review gate is not machine-checkable. A fully-fleshed-out plan is
# used as the deterministic proxy that the review concluded.
# NOTE: the Requirement Coverage heading is a /prism-plan addition. A feature whose tasks.md exists
# with no tasks/ directory predates the Prism layout (its plan came from the deprecated
# speckit-plan route), so - mirroring Test-State09 - the heading check is skipped rather than
# failed for it. A legacy feature that never reached tasks still reports as state 06.
function Test-State07 {
    param([string]$PlanMd, [string]$ResearchMd, [string]$DataModelMd, [string]$QuickstartMd, [string]$TasksMd, [string]$TasksDir)
    $planExists = Test-Path -LiteralPath $PlanMd -PathType Leaf
    $legacyLayout = (Test-Path -LiteralPath $TasksMd -PathType Leaf) -and
        -not (Test-Path -LiteralPath $TasksDir -PathType Container)
    $hasCoverage = $legacyLayout -or (Test-FileMatchesPattern -Path $PlanMd -Pattern '^#+\s*Requirement Coverage')
    $confirmed = $planExists -and
        -not (Test-FileContainsText -Path $PlanMd -Text $Placeholder) -and
        $hasCoverage -and
        (Test-Path -LiteralPath $ResearchMd -PathType Leaf) -and
        (Test-Path -LiteralPath $DataModelMd -PathType Leaf) -and
        (Test-Path -LiteralPath $QuickstartMd -PathType Leaf)
    [pscustomobject]@{ Confirmed = $confirmed; Evidence = $planExists }
}

# 08: Tasks in Progress -> confirmed once tasks.md exists (task generation has started).
function Test-State08 {
    param([string]$TasksMd)
    $exists = Test-Path -LiteralPath $TasksMd -PathType Leaf
    [pscustomobject]@{ Confirmed = $exists; Evidence = $exists }
}

# 09: Tasks Complete. Ready for Work Items -> confirmed once tasks.md has at least one task item
# and no outstanding placeholders, and - for the current /prism-tasks layout - tasks/ holds one
# file per index item. A feature with no tasks/ directory predates that layout (its task
# definitions sit inline in tasks.md), so the parity check is skipped rather than failed.
function Test-State09 {
    param([string]$TasksMd, [string]$TasksDir, [int]$TotalTaskItems)
    $exists = Test-Path -LiteralPath $TasksMd -PathType Leaf
    $taskFiles = @(Get-ChildItem -LiteralPath $TasksDir -Filter 'T*.md' -File -ErrorAction SilentlyContinue)
    $tasksDirExists = Test-Path -LiteralPath $TasksDir -PathType Container
    $filesMatchIndex = (-not $tasksDirExists) -or ($taskFiles.Count -eq $TotalTaskItems)
    $confirmed = $exists -and $TotalTaskItems -gt 0 -and $filesMatchIndex -and
        -not (Test-FileContainsText -Path $TasksMd -Text $Placeholder)
    [pscustomobject]@{ Confirmed = $confirmed; Evidence = $exists }
}

# 10: Work Items Created. Ready for Analysis -> confirmed once a work-item reference appears in
# tasks.md or in any task file under tasks/.
# NOTE: work items live in Azure DevOps/GitHub - an external system this script cannot query.
# This is a best-effort textual heuristic (a work-item link/id in the task artefacts), not
# authoritative proof that work items were actually created. The spec's
# azure-devops-parent-feature-id is deliberately NOT used as evidence here - per sdd.md it is
# filled in at spec time, well before tasks (or work items) exist.
function Test-State10 {
    param([string]$TasksMd, [string]$TasksDir)
    $confirmed = Test-FileMatchesPattern -Path $TasksMd -Pattern $WorkItemRefPattern
    if (-not $confirmed) {
        foreach ($taskFile in @(Get-ChildItem -LiteralPath $TasksDir -Filter 'T*.md' -File -ErrorAction SilentlyContinue)) {
            if (Test-FileMatchesPattern -Path $taskFile.FullName -Pattern $WorkItemRefPattern) { $confirmed = $true; break }
        }
    }
    [pscustomobject]@{ Confirmed = $confirmed; Evidence = $confirmed }
}

# 11: Ready for Implementation.
# NOTE: /speckit-analyze is explicitly read-only and produces no artefact of its own, so this
# script has no deterministic way to confirm it ran. Marked Unconfirmable; state 12 confirming
# is treated as proxy evidence that this one passed too.
function Test-State11 {
    [pscustomobject]@{ Confirmed = $false; Evidence = $false; Unconfirmable = $true }
}

# 12: Implementation in Progress -> confirmed once some but not all tasks are ticked, or code
# changes outside specs/ exist on the feature branch.
function Test-State12 {
    param([int]$TotalTaskItems, [int]$TickedTaskItems, [bool]$CodeDiffOutsideSpecs)
    $evidence = ($TickedTaskItems -gt 0) -or $CodeDiffOutsideSpecs
    $confirmed = $evidence -and $TotalTaskItems -gt 0 -and $TickedTaskItems -lt $TotalTaskItems
    [pscustomobject]@{ Confirmed = $confirmed; Evidence = $evidence }
}

# 13: Implementation Drifted.
# NOTE: detecting drift between the implementation and the SDD artefacts requires semantic
# comparison (this is exactly what /speckit-converge is for) - not something a file/git check
# can determine. Marked Unconfirmable; state 14 confirming is treated as proxy evidence that
# no drift occurred (or that it was resolved).
function Test-State13 {
    [pscustomobject]@{ Confirmed = $false; Evidence = $false; Unconfirmable = $true }
}

# 14: Implementation Complete. Ready for PR -> confirmed once every task is ticked and there
# are code changes outside specs/ on the feature branch.
function Test-State14 {
    param([int]$TotalTaskItems, [int]$TickedTaskItems, [bool]$CodeDiffOutsideSpecs)
    $allTicked = $TotalTaskItems -gt 0 -and $TickedTaskItems -eq $TotalTaskItems
    [pscustomobject]@{ Confirmed = ($allTicked -and $CodeDiffOutsideSpecs); Evidence = $allTicked }
}

# 15: Feature Merged.
# NOTE: uses "fully-ticked tasks, visible while sitting on the default branch" as the
# deterministic proxy for a merge; it cannot distinguish that from someone having manually
# checked out the default branch while the feature branch remains open elsewhere.
function Test-State15 {
    param([bool]$IsDefaultBranch, [int]$TotalTaskItems, [int]$TickedTaskItems)
    $confirmed = $IsDefaultBranch -and $TotalTaskItems -gt 0 -and $TickedTaskItems -eq $TotalTaskItems
    [pscustomobject]@{ Confirmed = $confirmed; Evidence = $confirmed }
}

# ============================================================================
# Gather context
# ============================================================================

$repoRoot = Get-RepoRoot
$defaultBranch = Get-DefaultBranch -RepoRoot $repoRoot
$currentBranch = Get-CurrentBranch -RepoRoot $repoRoot
$isDefaultBranch = [bool]($currentBranch -and $currentBranch -eq $defaultBranch)

if ($FeatureDirectory) {
    $resolvedFeatureDirectory = $FeatureDirectory
    if (-not [System.IO.Path]::IsPathRooted($resolvedFeatureDirectory)) { $resolvedFeatureDirectory = Join-Path $repoRoot $resolvedFeatureDirectory }
    $featureDir = if (Test-Path -LiteralPath $resolvedFeatureDirectory -PathType Container) { $resolvedFeatureDirectory } else { $null }
} else {
    $featureDir = Resolve-FeatureDir -RepoRoot $repoRoot -CurrentBranch $currentBranch
}

# ============================================================================
# States 00 / 01: no feature directory could be resolved at all
# ============================================================================

if (-not $featureDir -or -not (Test-Path -LiteralPath $featureDir -PathType Container)) {
    if ($isDefaultBranch) {
        $result = [ordered]@{
            featureDirectory            = $null
            latestConfirmedState        = [ordered]@{ id = 1; summary = 'Ready for New Feature' }
            latestUnconfirmedState      = $null
            unconfirmedStates           = @()
            blockedByUnconfirmableState = $null
        }
    } else {
        $result = [ordered]@{
            featureDirectory            = $null
            latestConfirmedState        = [ordered]@{ id = 0; summary = 'No Feature In Scope' }
            latestUnconfirmedState      = $null
            unconfirmedStates           = @()
            blockedByUnconfirmableState = $null
        }
    }
    $result | ConvertTo-Json -Depth 5
    return
}

# ============================================================================
# Feature directory resolved - evaluate states 02 through 15 in order
# ============================================================================

$specMd = Join-Path $featureDir 'spec.md'
$planMd = Join-Path $featureDir 'plan.md'
$tasksMd = Join-Path $featureDir 'tasks.md'
$researchMd = Join-Path $featureDir 'research.md'
$dataModelMd = Join-Path $featureDir 'data-model.md'
$quickstartMd = Join-Path $featureDir 'quickstart.md'
$tasksDir = Join-Path $featureDir 'tasks'
$checklistsDir = Join-Path $featureDir 'checklists'
$requirementsMd = Join-Path $checklistsDir 'requirements.md'
$clarificationsDir = Join-Path $featureDir 'clarifications'

$taskLines = @()
if (Test-Path -LiteralPath $tasksMd -PathType Leaf) { $taskLines = Get-Content -LiteralPath $tasksMd }
$totalTaskItems = @($taskLines | Select-String -Pattern $TaskItemPattern).Count
$tickedTaskItems = @($taskLines | Select-String -Pattern $TickedTaskItemPattern).Count
$codeDiffOutsideSpecs = Test-CodeDiffOutsideSpecs -RepoRoot $repoRoot -DefaultBranch $defaultBranch -IsDefaultBranch $isDefaultBranch

$stateDefs = @(
    @{ Id = 2;  Summary = 'Spec in Progress';                          Check = { Test-State02 $specMd } }
    @{ Id = 3;  Summary = 'Spec Drafted. Ready for Quality Checklist';  Check = { Test-State03 $specMd } }
    @{ Id = 4;  Summary = 'Spec Checked. Ready for Grilling';           Check = { Test-State04 $checklistsDir $requirementsMd } }
    @{ Id = 5;  Summary = 'Spec Complete. Ready for Technical Plan';    Check = { Test-State05 $featureDir $clarificationsDir } }
    @{ Id = 6;  Summary = 'Technical Plan in Progress';                 Check = { Test-State06 $planMd } }
    @{ Id = 7;  Summary = 'Technical Plan Complete. Ready for Tasks';   Check = { Test-State07 $planMd $researchMd $dataModelMd $quickstartMd $tasksMd $tasksDir } }
    @{ Id = 8;  Summary = 'Tasks in Progress';                          Check = { Test-State08 $tasksMd } }
    @{ Id = 9;  Summary = 'Tasks Complete. Ready for Work Items';       Check = { Test-State09 $tasksMd $tasksDir $totalTaskItems } }
    @{ Id = 10; Summary = 'Work Items Created. Ready for Analysis';    Check = { Test-State10 $tasksMd $tasksDir } }
    @{ Id = 11; Summary = 'Ready for Implementation';                  Check = { Test-State11 } }
    @{ Id = 12; Summary = 'Implementation in Progress';                Check = { Test-State12 $totalTaskItems $tickedTaskItems $codeDiffOutsideSpecs } }
    @{ Id = 13; Summary = 'Implementation Drifted';                    Check = { Test-State13 } }
    @{ Id = 14; Summary = 'Implementation Complete. Ready for PR';     Check = { Test-State14 $totalTaskItems $tickedTaskItems $codeDiffOutsideSpecs } }
    @{ Id = 15; Summary = 'Feature Merged';                            Check = { Test-State15 $isDefaultBranch $totalTaskItems $tickedTaskItems } }
)

$latestConfirmed = [ordered]@{ id = 1; summary = 'Ready for New Feature' }
$unconfirmedStates = @()
$pendingUnconfirmableId = $null  # holds a state id whose completion couldn't be checked directly
$blockedByUnconfirmable = $null  # set while an unverifiable gate is still capping the streak

foreach ($def in $stateDefs) {
    $checkResult = & $def.Check
    $isImmediateNext = ($def.Id -eq ($latestConfirmed.id + 1))
    $isProxyNext = ($null -ne $pendingUnconfirmableId) -and ($def.Id -eq ($pendingUnconfirmableId + 1))

    # A feature branch must be checked out for states 2-14 to be confirmed OR count as
    # evidence; state 15 is the sole exception (it explicitly requires the default branch, as
    # a merge signal). A stale feature.json pin combined with sitting on the default branch
    # must read as no work having started, not as unconfirmed work in progress.
    $confirmable = ($def.Id -eq 15) -or (-not $isDefaultBranch)
    $checkResult.Confirmed = $checkResult.Confirmed -and $confirmable
    $checkResult.Evidence = $checkResult.Evidence -and $confirmable

    if ($isImmediateNext -and $checkResult.Confirmed) {
        $latestConfirmed = [ordered]@{ id = $def.Id; summary = $def.Summary }
        $pendingUnconfirmableId = $null
    } elseif ($isImmediateNext -and $checkResult.Unconfirmable) {
        # can't verify this state directly; give the next state a chance to act as proxy
        $pendingUnconfirmableId = $def.Id
        $blockedByUnconfirmable = [ordered]@{ id = $def.Id; summary = $def.Summary }
    } elseif ($isProxyNext -and $checkResult.Confirmed) {
        # this state confirming implies the unconfirmable gap before it passed too
        $latestConfirmed = [ordered]@{ id = $def.Id; summary = $def.Summary }
        $pendingUnconfirmableId = $null
        $blockedByUnconfirmable = $null
    } else {
        if ($isProxyNext) { $pendingUnconfirmableId = $null }
        if ($checkResult.Evidence) {
            $unconfirmedStates += [ordered]@{ id = $def.Id; summary = $def.Summary }
        }
    }
}

$latestUnconfirmed = if ($unconfirmedStates.Count -gt 0) { $unconfirmedStates[-1] } else { $null }

$featureDirectoryRelative = $featureDir -replace [regex]::Escape($repoRoot + [IO.Path]::DirectorySeparatorChar), ''

$result = [ordered]@{
    featureDirectory            = $featureDirectoryRelative
    latestConfirmedState        = $latestConfirmed
    latestUnconfirmedState      = $latestUnconfirmed
    unconfirmedStates           = $unconfirmedStates
    blockedByUnconfirmableState = $blockedByUnconfirmable
}

$result | ConvertTo-Json -Depth 5
