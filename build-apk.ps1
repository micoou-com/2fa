#Requires -Version 5.1
<#
.SYNOPSIS
  Build a Release APK for TwoFactorAuthApp.

.EXAMPLE
  .\build-apk.ps1
  .\build-apk.ps1 -Install
#>
[CmdletBinding()]
param(
    [switch]$Install
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot = $PSScriptRoot
$Project = Join-Path $RepoRoot 'src\TwoFactorAuthApp\TwoFactorAuthApp.csproj'
$PublishDir = Join-Path $RepoRoot 'src\TwoFactorAuthApp\bin\Release\net9.0-android\publish'

function Resolve-DotNet {
    $candidates = @(
        'C:\Program Files\dotnet\dotnet.exe',
        (Join-Path $env:ProgramFiles 'dotnet\dotnet.exe')
    )
    if ($env:DOTNET_ROOT) {
        $candidates = @((Join-Path $env:DOTNET_ROOT 'dotnet.exe')) + $candidates
    }
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd) { $candidates += $cmd.Source }

    foreach ($path in ($candidates | Select-Object -Unique)) {
        if ($path -and (Test-Path -LiteralPath $path)) {
            return (Resolve-Path -LiteralPath $path).Path
        }
    }

    throw 'dotnet CLI not found. Install .NET 9 SDK: https://dotnet.microsoft.com/download'
}

function Ensure-AndroidWorkload([string]$DotNetExe) {
    $list = & $DotNetExe workload list 2>&1 | Out-String
    if ($list -notmatch '\bandroid\b') {
        Write-Host 'Android workload not installed. Running: dotnet workload restore ...' -ForegroundColor Yellow
        Push-Location $RepoRoot
        try {
            & $DotNetExe workload restore $Project
        }
        finally {
            Pop-Location
        }
    }
}

function Find-Apk([string]$Directory) {
    Get-ChildItem -LiteralPath $Directory -Filter '*.apk' -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending
}

$dotnet = Resolve-DotNet
Write-Host "Using dotnet: $dotnet" -ForegroundColor Cyan

$env:DOTNET_ROOT = Split-Path -Parent $dotnet
Ensure-AndroidWorkload $dotnet

Write-Host 'Publishing Release APK ...' -ForegroundColor Cyan
Push-Location $RepoRoot
try {
    & $dotnet publish $Project `
        -c Release `
        -f net9.0-android `
        -p:AndroidPackageFormats=apk
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

if (-not (Test-Path -LiteralPath $PublishDir)) {
    throw "Publish directory not found: $PublishDir"
}

$apks = Find-Apk $PublishDir
if (-not $apks) {
    throw "No APK found under $PublishDir"
}

$signed = $apks | Where-Object { $_.Name -like '*-Signed.apk' } | Select-Object -First 1
$preferred = if ($signed) { $signed } else { $apks[0] }

Write-Host ''
Write-Host 'Build succeeded.' -ForegroundColor Green
Write-Host "Recommended: $($preferred.FullName) ($([math]::Round($preferred.Length / 1MB, 2)) MB)"
foreach ($apk in $apks) {
    if ($apk.FullName -ne $preferred.FullName) {
        Write-Host "Also:        $($apk.FullName) ($([math]::Round($apk.Length / 1MB, 2)) MB)"
    }
}

if ($Install) {
    $adb = Get-Command adb -ErrorAction SilentlyContinue
    if (-not $adb) {
        throw 'adb not found in PATH. Install Android platform-tools or run without -Install.'
    }
    Write-Host ''
    Write-Host "Installing $($preferred.Name) ..." -ForegroundColor Cyan
    & $adb.Source install -r $preferred.FullName
    if ($LASTEXITCODE -ne 0) {
        throw "adb install failed with exit code $LASTEXITCODE"
    }
    Write-Host 'Installed on connected device.' -ForegroundColor Green
}
