# Contact Manager - Local Startup Script (PowerShell)
# Usage: .\start-local.ps1

$ErrorActionPreference = "Stop"

# Colors
$Green = "`e[32m"
$Red = "`e[31m"
$Yellow = "`e[33m"
$Reset = "`e[0m"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Contact Manager - Local Startup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    $majorVersion = ($dotnetVersion -split '\.')[0]
    
    if ([int]$majorVersion -lt 8) {
        Write-Host "${Red}Error: .NET 8 or higher required. Current: $dotnetVersion${Reset}"
        Write-Host "Install from: https://dotnet.microsoft.com/download"
        exit 1
    }
    
    Write-Host "${Green}✓ .NET SDK found: $dotnetVersion${Reset}"
} catch {
    Write-Host "${Red}Error: .NET SDK not installed${Reset}"
    Write-Host "Install from: https://dotnet.microsoft.com/download"
    exit 1
}

# Set paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
$webProject = Join-Path $projectRoot "ContactManager.Web"
$solution = Join-Path $projectRoot "ContactManager.sln"

Write-Host ""
Write-Host "Project root: $projectRoot"
Write-Host "Web project: $webProject"

# Restore
Write-Host ""
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore $solution --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "${Red}✗ Restore failed${Reset}"
    exit 1
}
Write-Host "${Green}✓ Packages restored${Reset}"

# Build
Write-Host ""
Write-Host "Building..." -ForegroundColor Yellow
dotnet build $solution --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "${Red}✗ Build failed${Reset}"
    exit 1
}
Write-Host "${Green}✓ Build successful${Reset}"

# Start
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Starting Contact Manager..." -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Application will be available at:" -ForegroundColor Yellow
Write-Host "  http://localhost:5021" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

Push-Location $webProject
dotnet run --launch-profile "http" --verbosity quiet
Pop-Location

Write-Host ""
Write-Host "Application stopped" -ForegroundColor Yellow
