#!/usr/bin/env pwsh
# Shared by Prism's own scripts. They ship in the plugin as well as in the repository, so they find
# the project from the working directory, never from their own location, and Prism's files from
# their own location.

# The project root: the nearest folder above the working directory holding `.specify/` (as Spec
# Kit finds it), then the git work tree, then the working directory.
function Get-PrismProjectRoot {
    $current = (Get-Location).Path
    while ($current) {
        if (Test-Path -LiteralPath (Join-Path $current '.specify') -PathType Container) { return $current }
        $parent = Split-Path $current -Parent
        if (-not $parent -or $parent -eq $current) { break }
        $current = $parent
    }
    $top = git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -eq 0 -and $top) { return $top }
    return (Get-Location).Path
}

# Whether git is installed and the project is a git work tree. Prism's own, because Spec Kit 1.0.4
# dropped its Test-HasGit.
function Test-PrismHasGit([string]$ProjectRoot) {
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) { return $false }
    git -C $ProjectRoot rev-parse --is-inside-work-tree 2>$null | Out-Null
    return $LASTEXITCODE -eq 0
}

# Prism's own files: the plugin, or the repository on the file-based route.
$PrismRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../..')).Path

# Loads Spec Kit's shared functions from the project. Spec Kit owns common.ps1; Prism reads it but
# never modifies it.
function Import-SpecKitCommon([string]$ProjectRoot) {
    $common = Join-Path $ProjectRoot '.specify/scripts/powershell/common.ps1'
    if (-not (Test-Path -LiteralPath $common)) {
        [Console]::Error.WriteLine("ERROR: Spec Kit helper not found: $common")
        [Console]::Error.WriteLine("Run Prism's setup agent to install Spec Kit's files in this project.")
        exit 1
    }
    return $common
}
