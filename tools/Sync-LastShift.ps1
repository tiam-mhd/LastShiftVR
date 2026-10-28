<#
.SYNOPSIS
Synchronize the complete LastShift Unity Assets folder from this GitHub checkout.
.DESCRIPTION
Close Unity first. The script MOVES the existing Assets/LastShift into
a timestamped backup folder OUTSIDE Assets, then copies the current
repository's Assets/LastShift. This prevents deleted DEBUG and fake-3D
scripts from lingering after a normal Windows folder merge.
.PARAMETER UnityProject
Absolute or relative path to your Unity project folder (contains Assets).
.EXAMPLE
powershell -ExecutionPolicy Bypass -File .\tools\Sync-LastShift.ps1 -UnityProject "D:\UnityProjects\LastShiftVR"
#>
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$UnityProject)
$ErrorActionPreference = "Stop"

$source = Join-Path (Split-Path -Parent $PSScriptRoot) "Assets\LastShift"
$project = [IO.Path]::GetFullPath($UnityProject)
$assetRoot = Join-Path $project "Assets"
$destination = Join-Path $assetRoot "LastShift"

if (-not (Test-Path -LiteralPath (Join-Path $source "Editor\Ward13PatientBuilder.cs"))) {
    throw "Repository source not found at $source. Extract the latest GitHub ZIP first."
}
if (-not (Test-Path -LiteralPath $assetRoot)) {
    throw "Not a Unity project: $assetRoot does not exist."
}
if ([IO.Path]::GetFullPath($source).TrimEnd('\') -eq [IO.Path]::GetFullPath($destination).TrimEnd('\')) {
    throw "Source and destination are identical. Use a separate GitHub checkout."
}
if (Test-Path -LiteralPath $destination) {
    $backupRoot = Join-Path $project "LastShift_Backups"
    if (-not (Test-Path -LiteralPath $backupRoot)) {
        New-Item -ItemType Directory -Path $backupRoot | Out-Null
    }
    $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backup = Join-Path $backupRoot "LastShift_$stamp"
    if (Test-Path -LiteralPath $backup) {
        $backup += "_" + [guid]::NewGuid().ToString("N").Substring(0,6)
    }
    Move-Item -LiteralPath $destination -Destination $backup
    Write-Host "Backed up old Unity LastShift folder to: $backup"
}
Copy-Item -LiteralPath $source -Destination $destination -Recurse
Write-Host "Installed current LastShift source in $destination"
Write-Host "Old DEBUG and fake-3D scripts will no longer appear after opening Unity."
Write-Host "Open Unity, wait for scripts to compile, and run:"
Write-Host "Tools > Last Shift > Build Final Ward 13 - Real Patient FBX"
