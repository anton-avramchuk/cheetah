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

# Effective version used for both build and pack (override wins, otherwise version.props).
$effectiveVersion = $Version
if (-not $effectiveVersion) {
    [xml]$vp = Get-Content (Join-Path $PSScriptRoot "version.props")
    $effectiveVersion = ($vp.Project.PropertyGroup.CheetahVersion | Select-Object -First 1).ToString().Trim()
}

$buildArgs = @("build", $SolutionFile, "--configuration", $Configuration)
if ($Version) {
    # Stamp assemblies with the same version as the package — otherwise build emits
    # version.props version while pack labels the package $Version, producing a nupkg
    # whose dll AssemblyVersion does not match the package version.
    $buildArgs += "/p:CheetahVersion=$Version"
}

Write-Host "Building solution (version $effectiveVersion)..."
dotnet @buildArgs
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

# Guard: assembly version inside each freshly packed nupkg must match the package version.
# Catches the "build at old version, pack at new version" mismatch that ships a stale dll.
Write-Host ""
Write-Host "Verifying assembly version == package version ($effectiveVersion)..."
Add-Type -AssemblyName System.IO.Compression.FileSystem
$mismatches = @()
$checked = 0
foreach ($pkg in Get-ChildItem (Join-Path $OutputPath "Cheetah.*.$effectiveVersion.nupkg")) {
    $id = $pkg.Name -replace ("\." + [regex]::Escape($effectiveVersion) + "\.nupkg$"), ""
    $zip = [System.IO.Compression.ZipFile]::OpenRead($pkg.FullName)
    try {
        $entry = $zip.Entries | Where-Object { $_.Name -ieq "$id.dll" } | Select-Object -First 1
        if ($null -eq $entry) { continue }   # metapackage / no own assembly
        $tmp = Join-Path $env:TEMP ("pkverify_" + [guid]::NewGuid() + ".dll")
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $tmp, $true)
        $asmVer = [System.Reflection.AssemblyName]::GetAssemblyName($tmp).Version
        Remove-Item $tmp -Force -ErrorAction SilentlyContinue
        $checked++
        $short = "$($asmVer.Major).$($asmVer.Minor).$($asmVer.Build)"
        if ($short -ne $effectiveVersion) {
            $mismatches += "  ${id}: package=$effectiveVersion  assembly=$asmVer"
        }
    } finally { $zip.Dispose() }
}

if ($mismatches.Count -gt 0) {
    Write-Error ("Version mismatch in $($mismatches.Count) package(s) — assembly version != package version:`n" +
        ($mismatches -join "`n") +
        "`nLikely cause: pack reused stale build outputs. Do a clean rebuild with the same CheetahVersion.")
    exit 1
}

Write-Host "OK: $checked package(s) verified — assembly version matches $effectiveVersion."
Write-Host ""
Write-Host "Done. Packages written to $OutputPath"
