#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Pack all Cheetah.* libraries and publish to local feed at E:\packages.
.PARAMETER Version
    Override the package version (default: reads from version.props).
.PARAMETER Configuration
    Build configuration (default: Release).
#>
param(
    [string]$Version,
    [string]$Configuration = "Release"
)

$OutputPath = "E:\packages"
$SolutionFile = Join-Path $PSScriptRoot "Cheetah.slnx"

# Ensure output directory exists
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
    Write-Host "Created local feed directory: $OutputPath"
}

$packArgs = @(
    "pack", $SolutionFile,
    "--configuration", $Configuration,
    "--output", $OutputPath,
    "--no-restore"
)

if ($Version) {
    $packArgs += @("/p:CheetahVersion=$Version")
}

Write-Host "Building solution..."
dotnet build $SolutionFile --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Aborting pack."
    exit 1
}

Write-Host ""
Write-Host "Packing Cheetah.* packages -> $OutputPath"
dotnet @packArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Pack failed."
    exit 1
}

Write-Host ""
Write-Host "Done. Packages written to $OutputPath"
