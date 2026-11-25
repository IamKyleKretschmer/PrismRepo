#!/usr/bin/env pwsh
# Validate that changes to spec.md have been propagated to plan.md, tasks.md and
# implementation-status.md.
#
# Propagation is checked by comparing when each artefact last changed: an artefact
# derived from spec.md must not be older than spec.md. "Last changed" is the last
# commit time of the artefact, or its working-tree modification time when it has
# uncommitted changes -- so a spec.md edited but not yet committed still counts as
# newer than a plan generated from an earlier version of it.
#
# Artefacts changed in the same commit share a timestamp and compare as up to date,
# so regenerating a plan and committing it alongside the spec passes.

[CmdletBinding()]
param(
    [switch]$All,
    [switch]$Json,
    [switch]$Strict,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

if ($Help) {
    Write-Output "Usage: ./validate-spec-propagation.ps1 [-All] [-Json] [-Strict]"
    Write-Output "  Default: validates the current feature (resolved the same way as the SDD skills)."
    Write-Output "  -All    Validates every feature folder in /specs that contains a spec.md."
    Write-Output "  -Strict Fails if plan/tasks are missing where expected."
    exit 0
}

# Finds the project from the working directory, so this runs from the plugin as well as in place.
. (Join-Path $PSScriptRoot 'prism-common.ps1')
. (Import-SpecKitCommon (Get-PrismProjectRoot))

# Last-changed time for one artefact, as UTC. Returns $null when the file does not exist.
#
# Uncommitted changes win over commit history, because the edit that creates drift is
# usually still in the working tree when the validator runs.
function Get-ArtefactTimestamp {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$RepoRoot,
        [bool]$HasGit = $true
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }

    $mtime = (Get-Item -LiteralPath $Path).LastWriteTimeUtc
    if (-not $HasGit) { return $mtime }

    try {
        $dirty = git -C $RepoRoot status --porcelain -- $Path 2>$null
        if ($dirty) { return $mtime }

        $committed = git -C $RepoRoot log -1 --format=%cI -- $Path 2>$null
        if ([string]::IsNullOrWhiteSpace($committed)) { return $mtime }

        return ([datetimeoffset]::Parse($committed.Trim())).UtcDateTime
    } catch {
        # Any git failure (missing binary, unreadable path) falls back to the
        # filesystem, which is always available.
        return $mtime
    }
}

function Format-Timestamp {
    param([datetime]$Value)
    return $Value.ToString("yyyy-MM-dd HH:mm:ss") + "Z"
}

# $DerivedName must not predate $SourceName. Equal timestamps (same commit) are up to date.
function Test-Propagated {
    param(
        [Parameter(Mandatory=$true)][string]$SourceName,
        [Parameter(Mandatory=$true)][datetime]$SourceTime,
        [Parameter(Mandatory=$true)][string]$DerivedName,
        [Parameter(Mandatory=$true)][datetime]$DerivedTime
    )

    if ($DerivedTime -lt $SourceTime) {
        return "$DerivedName (last changed $(Format-Timestamp $DerivedTime)) is older than $SourceName (last changed $(Format-Timestamp $SourceTime)) -- regenerate it or confirm it is still accurate"
    }

    return $null
}

function New-Result {
    param(
        [Parameter(Mandatory=$true)][string]$Feature,
        [Parameter(Mandatory=$true)][string]$Status,
        [string[]]$Errors,
        [string[]]$Warnings
    )

    [PSCustomObject]@{
        Feature = $Feature
        Status = $Status
        Errors = ($Errors | Where-Object { $_ })
        Warnings = ($Warnings | Where-Object { $_ })
    }
}

function Validate-Feature {
    param(
        [Parameter(Mandatory=$true)][string]$FeatureDir,
        [Parameter(Mandatory=$true)][string]$FeatureName,
        [Parameter(Mandatory=$true)][string]$RepoRoot,
        [Parameter(Mandatory=$true)][bool]$StrictMode,
        [bool]$HasGit = $true
    )

    $errors = @()
    $warnings = @()

    $specPath = Join-Path $FeatureDir "spec.md"
    $planPath = Join-Path $FeatureDir "plan.md"
    $tasksPath = Join-Path $FeatureDir "tasks.md"
    $statusPath = Join-Path $FeatureDir "implementation-status.md"
    $taskFilesDir = Join-Path $FeatureDir "tasks"

    $specTime = Get-ArtefactTimestamp -Path $specPath -RepoRoot $RepoRoot -HasGit $HasGit
    if (-not $specTime) {
        $errors += "Missing spec.md"
        return (New-Result -Feature $FeatureName -Status "fail" -Errors $errors -Warnings $warnings)
    }

    $planTime = Get-ArtefactTimestamp -Path $planPath -RepoRoot $RepoRoot -HasGit $HasGit
    $tasksTime = Get-ArtefactTimestamp -Path $tasksPath -RepoRoot $RepoRoot -HasGit $HasGit

    if ($planTime) {
        $errors += Test-Propagated -SourceName "spec.md" -SourceTime $specTime -DerivedName "plan.md" -DerivedTime $planTime
    } elseif ($StrictMode) {
        $warnings += "plan.md not found"
    }

    if ($tasksTime) {
        $errors += Test-Propagated -SourceName "spec.md" -SourceTime $specTime -DerivedName "tasks.md" -DerivedTime $tasksTime
        if ($planTime) {
            $errors += Test-Propagated -SourceName "plan.md" -SourceTime $planTime -DerivedName "tasks.md" -DerivedTime $tasksTime
        }

        $statusTime = Get-ArtefactTimestamp -Path $statusPath -RepoRoot $RepoRoot -HasGit $HasGit
        if ($statusTime) {
            $errors += Test-Propagated -SourceName "spec.md" -SourceTime $specTime -DerivedName "implementation-status.md" -DerivedTime $statusTime
        } else {
            $errors += "implementation-status.md missing (required once tasks.md exists)"
        }

        # Per-task files (prism-tasks writes tasks/T<NNN>-<slug>.md) are reported as
        # warnings: a spec change often affects only some of them.
        if (Test-Path -LiteralPath $taskFilesDir -PathType Container) {
            Get-ChildItem -LiteralPath $taskFilesDir -Filter "*.md" -File | ForEach-Object {
                $taskTime = Get-ArtefactTimestamp -Path $_.FullName -RepoRoot $RepoRoot -HasGit $HasGit
                if ($taskTime -and $taskTime -lt $specTime) {
                    $warnings += "tasks/$($_.Name) (last changed $(Format-Timestamp $taskTime)) is older than spec.md -- check it is still accurate"
                }
            }
        }
    } elseif ($StrictMode) {
        $warnings += "tasks.md not found"
    }

    $errors = @($errors | Where-Object { $_ })
    $status = if ($errors.Count -gt 0) { "fail" } else { "pass" }
    return (New-Result -Feature $FeatureName -Status $status -Errors $errors -Warnings $warnings)
}

$results = @()
$repoRoot = Get-RepoRoot
$hasGit = Test-PrismHasGit (Get-PrismProjectRoot)

if ($All) {
    $specsRoot = Join-Path $repoRoot "specs"

    if (-not (Test-Path $specsRoot)) {
        Write-Error "Specs directory not found: $specsRoot"
        exit 1
    }

    # Any folder holding a spec.md is a feature, whatever featureFolderFormat names it.
    Get-ChildItem -Path $specsRoot -Directory | ForEach-Object {
        if (-not (Test-Path -LiteralPath (Join-Path $_.FullName "spec.md") -PathType Leaf)) { return }
        $results += Validate-Feature -FeatureDir $_.FullName -FeatureName $_.Name -RepoRoot $repoRoot -StrictMode:$Strict -HasGit $hasGit
    }

    if ($results.Count -eq 0) {
        Write-Error "No feature folders containing a spec.md found under $specsRoot"
        exit 1
    }
} else {
    $paths = Get-FeaturePathsEnv
    if (-not (Test-Path -LiteralPath $paths.FEATURE_DIR -PathType Container)) {
        [Console]::Error.WriteLine("ERROR: Feature directory not found: $($paths.FEATURE_DIR)")
        exit 1
    }
    $featureName = Split-Path $paths.FEATURE_DIR -Leaf
    $results += Validate-Feature -FeatureDir $paths.FEATURE_DIR -FeatureName $featureName -RepoRoot $repoRoot -StrictMode:$true -HasGit $hasGit
}

if ($Json) {
    $results | ConvertTo-Json -Depth 5 -Compress
    exit 0
}

$failed = $false
foreach ($r in $results) {
    if ($r.Status -eq 'pass') {
        Write-Output "PASS: $($r.Feature) - spec propagation OK"
    } else {
        $failed = $true
        Write-Output "FAIL: $($r.Feature) - spec propagation FAILED"
        foreach ($e in $r.Errors) { Write-Output "  - ERROR: $e" }
    }

    foreach ($w in $r.Warnings) { Write-Output "  - WARN: $w" }
}

if ($failed) { exit 1 }
