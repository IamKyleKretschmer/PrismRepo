#!/usr/bin/env pwsh
# Resolves an SDD template for Prism's phases: the team's override, then Prism's own template,
# then Spec Kit's base template in the project.
#
# Prism's templates are found relative to this script, so the lookup works from the plugin and
# in place. The project is found from the working directory (or given as -ProjectRoot).

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Name,
    [string]$ProjectRoot,
    # A file a Spec Kit setup script has just filled from Spec Kit's resolution. It is replaced
    # with the resolved template while it is still an untouched copy of Spec Kit's base.
    [string]$ReplaceSpecKitCopy,
    [switch]$Json
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'prism-common.ps1')

if ($Name -cnotmatch '^[a-z0-9-]+$') {
    [Console]::Error.WriteLine("ERROR: Invalid template name '$Name'.")
    exit 1
}

if (-not $ProjectRoot) { $ProjectRoot = Get-PrismProjectRoot }

$prismTemplates = Join-Path $PrismRoot '.prism/templates'
$layers = @(
    @{ Layer = 'team-override'; Path = Join-Path $ProjectRoot ".specify/templates/overrides/$Name.md" },
    @{ Layer = 'prism'; Path = Join-Path $prismTemplates "$Name.md" },
    @{ Layer = 'spec-kit'; Path = Join-Path $ProjectRoot ".specify/templates/$Name.md" }
)
$found = $layers | Where-Object { Test-Path -LiteralPath $_.Path -PathType Leaf } | Select-Object -First 1
if (-not $found) {
    [Console]::Error.WriteLine("ERROR: Template '$Name' not found in the team overrides, Prism's templates or Spec Kit's base templates.")
    exit 1
}
$template = (Resolve-Path -LiteralPath $found.Path).Path

function Read-Normalised([string]$Path) {
    return ([System.IO.File]::ReadAllText($Path) -replace "`r`n", "`n")
}

$replaced = $false
if ($ReplaceSpecKitCopy -and (Test-Path -LiteralPath $ReplaceSpecKitCopy -PathType Leaf)) {
    $base = $layers[2].Path
    $isBaseCopy = (Test-Path -LiteralPath $base -PathType Leaf) -and
        ((Read-Normalised $ReplaceSpecKitCopy) -eq (Read-Normalised $base))
    if ($isBaseCopy -and $found.Layer -ne 'spec-kit') {
        Copy-Item -LiteralPath $template -Destination $ReplaceSpecKitCopy -Force
        $replaced = $true
    }
}

if ($Json) {
    [PSCustomObject]@{ TEMPLATE = $template; LAYER = $found.Layer; REPLACED = $replaced } | ConvertTo-Json -Compress
} else {
    Write-Output "TEMPLATE: $template"
    Write-Output "LAYER: $($found.Layer)"
    if ($ReplaceSpecKitCopy) { Write-Output "REPLACED: $replaced" }
}
