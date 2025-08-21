# Hardcoded publish script for Jira.Api
# Expects nuget_key.txt (ignored by git) at repo root containing ONLY the API key.

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Fail($msg) {
    Write-Error $msg
    exit 1
}

$RepoRoot   = $PSScriptRoot
$ApiKeyFile = Join-Path $RepoRoot 'nuget_key.txt'
$Project    = Join-Path $RepoRoot 'Jira.Api/Jira.Api.csproj'
$Configuration = 'Release'
$OutputDir  = Join-Path $RepoRoot 'artifacts/nuget'

if (-not (Test-Path -Path $ApiKeyFile -PathType Leaf)) { Fail "API key file 'nuget_key.txt' not found in repository root. Aborting." }
$apiKey = (Get-Content -Path $ApiKeyFile -Raw).Trim()
if ([string]::IsNullOrWhiteSpace($apiKey)) { Fail "API key file 'nuget_key.txt' is empty. Aborting." }
if (-not (Test-Path -Path $Project -PathType Leaf)) { Fail "Project file '$Project' not found. Aborting." }

Write-Host "--- Restoring packages ---" -ForegroundColor Cyan
& dotnet restore $Project

Write-Host "--- Building ($Configuration) ---" -ForegroundColor Cyan
& dotnet build $Project -c $Configuration --no-restore

Write-Host "--- Packing ($Configuration) ---" -ForegroundColor Cyan
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
& dotnet pack $Project -c $Configuration -o $OutputDir --no-build
if ($LASTEXITCODE -ne 0) { Fail "dotnet pack failed." }

$packageId = 'Jira.Api'
$primaryPackages = Get-ChildItem -Path $OutputDir -Filter "${packageId}.*.nupkg" | Where-Object { $_.Name -notmatch '\.symbols\.nupkg$' -and $_.Name -notmatch '\.snupkg$' }
$symbolPackages  = @(Get-ChildItem -Path $OutputDir -Filter "${packageId}.*.snupkg" -ErrorAction SilentlyContinue) + @(Get-ChildItem -Path $OutputDir -Filter "${packageId}.*.symbols.nupkg" -ErrorAction SilentlyContinue)
if (-not $primaryPackages) { Fail "No primary package (.nupkg) found in '$OutputDir'." }

$nugetSource = 'https://api.nuget.org/v3/index.json'
Write-Host "--- Pushing primary packages to NuGet ---" -ForegroundColor Cyan
foreach ($pkg in $primaryPackages) {
    Write-Host "Pushing $($pkg.Name)" -ForegroundColor Green
    & dotnet nuget push $pkg.FullName --api-key $apiKey --source $nugetSource --skip-duplicate
}

if ($symbolPackages) {
    Write-Host "--- Pushing symbol packages to NuGet ---" -ForegroundColor Cyan
    foreach ($sym in $symbolPackages) {
        Write-Host "Pushing symbols $($sym.Name)" -ForegroundColor Green
        & dotnet nuget push $sym.FullName --api-key $apiKey --source $nugetSource --skip-duplicate
    }
} else {
    Write-Host 'No symbol packages found (snupkg / symbols.nupkg).' -ForegroundColor Yellow
}

Write-Host '--- Publish complete ---' -ForegroundColor Cyan
