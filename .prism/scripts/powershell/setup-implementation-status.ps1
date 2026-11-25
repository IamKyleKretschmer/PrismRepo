#!/usr/bin/env pwsh
# Setup implementation-status.md for a feature (traceability from spec → code)

[CmdletBinding()]
param(
    [switch]$Json,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

if ($Help) {
    Write-Output "Usage: ./setup-implementation-status.ps1 [-Json] [-Help]"
    exit 0
}

# Finds the project from the working directory, so this runs from the plugin as well as in place.
. (Join-Path $PSScriptRoot 'prism-common.ps1')
. (Import-SpecKitCommon (Get-PrismProjectRoot))

$paths = Get-FeaturePathsEnv

$targetFile = Join-Path $paths.FEATURE_DIR "implementation-status.md"

# The team's seed-once override in the project wins over Prism's template, mirroring the
# .prism/instructions/overrides/ convention.
$template = Join-Path $paths.REPO_ROOT ".prism/templates/overrides/implementation-status-template.md"
if (-not (Test-Path -LiteralPath $template)) {
    $template = Join-Path $PrismRoot ".prism/templates/implementation-status-template.md"
}

if (-not (Test-Path $template)) {
    Write-Error "Template not found: $template"
    exit 1
}

New-Item -ItemType Directory -Path $paths.FEATURE_DIR -Force | Out-Null
Copy-Item -LiteralPath $template -Destination $targetFile -Force

function Escape-Replacement {
    param([Parameter(Mandatory=$true)][string]$Value)
    return $Value -replace "\$", "$$"
}

function Get-FeatureTitle {
    param([Parameter(Mandatory=$true)][string]$SpecPath)

    if (-not (Test-Path $SpecPath)) { return "" }
    $lines = Get-Content -LiteralPath $SpecPath -Encoding utf8 -TotalCount 30
    foreach ($line in $lines) {
        if ($line -match "^#\s+Feature Specification:\s*(.+)\s*$") { return $Matches[1].Trim() }
    }

    return ""
}

try {
    $today = Get-Date
    $featureTitle = Get-FeatureTitle -SpecPath $paths.FEATURE_SPEC

    $content = Get-Content -LiteralPath $targetFile -Raw -Encoding utf8

    if ($featureTitle) {
        $content = $content -replace "\[FEATURE\]", (Escape-Replacement -Value $featureTitle)
    }

    $content = $content -replace "\[###-feature-name\]", (Escape-Replacement -Value $paths.CURRENT_BRANCH)
    $content = $content -replace "\[DATE\]", (Escape-Replacement -Value $today.ToString("yyyy-MM-dd"))

    Set-Content -LiteralPath $targetFile -Value $content -Encoding utf8
} catch {
    Write-Warning "[specify] Placeholder replacement failed for implementation-status.md: $_"
}

if ($Json) {
    [PSCustomObject]@{
        IMPLEMENTATION_STATUS = $targetFile
        BRANCH = $paths.CURRENT_BRANCH
    } | ConvertTo-Json -Compress
} else {
    Write-Output "IMPLEMENTATION_STATUS: $targetFile"
    Write-Output "BRANCH: $($paths.CURRENT_BRANCH)"
}
