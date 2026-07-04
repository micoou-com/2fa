#Requires -Version 5.1
<#
.SYNOPSIS
  Push current branch to GitHub (bypasses broken global github.com URL rewrite).

.EXAMPLE
  .\push-github.ps1
  .\push-github.ps1 -Branch master -Target main
#>
[CmdletBinding()]
param(
    [string]$Branch = 'develop',
    [string]$Target = 'develop'
)

$ErrorActionPreference = 'Stop'
$RepoRoot = $PSScriptRoot
$GitHubUrl = 'https://github.com/micoou-com/2fa.git'

Push-Location $RepoRoot
try {
    if (-not (git remote | Select-String -Pattern '^github$' -Quiet)) {
        git remote add github $GitHubUrl
    }
    else {
        git remote set-url github $GitHubUrl
    }

    # Skip global gitconfig that rewrites github.com -> hub.fastgit.xyz (broken mirror)
    $env:GIT_CONFIG_GLOBAL = 'NUL'
    $env:GIT_CONFIG_SYSTEM = 'NUL'

    Write-Host "Pushing $Branch -> github/$Target ..." -ForegroundColor Cyan
    git push -u github "${Branch}:${Target}"
    Write-Host "Done: https://github.com/micoou-com/2fa" -ForegroundColor Green
}
finally {
    Pop-Location
    Remove-Item Env:GIT_CONFIG_GLOBAL -ErrorAction SilentlyContinue
    Remove-Item Env:GIT_CONFIG_SYSTEM -ErrorAction SilentlyContinue
}
