@echo off
chcp 65001 >nul
:: Start Contact Manager Application locally on Windows
:: Usage: Double-click or run: start-local.bat

echo ==========================================
echo Contact Manager - Local Startup (Windows)
echo ==========================================

:: Check if .NET is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK is not installed
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

:: Get current directory and project paths
set "SCRIPT_DIR=%~dp0"
set "PROJECT_ROOT=%SCRIPT_DIR%.."
set "WEB_PROJECT=%PROJECT_ROOT%\ContactManager.Web"
set "SOLUTION=%PROJECT_ROOT%\ContactManager.sln"

echo.
echo [OK] .NET SDK found
echo Project root: %PROJECT_ROOT%
echo Web project: %WEB_PROJECT%

:: Restore packages
echo.
echo [INFO] Restoring NuGet packages...
dotnet restore "%SOLUTION%" --verbosity quiet
if errorlevel 1 (
    echo [ERROR] Failed to restore packages
    pause
    exit /b 1
)
echo [OK] Packages restored

:: Build project
echo.
echo [INFO] Building project...
dotnet build "%SOLUTION%" --verbosity quiet
if errorlevel 1 (
    echo [ERROR] Build failed
    pause
    exit /b 1
)
echo [OK] Build successful

:: Start application
echo.
echo ==========================================
echo [OK] Starting Contact Manager...
echo ==========================================
echo.
echo Application will be available at:
echo   http://localhost:5021
echo.
echo Press Ctrl+C to stop the server
echo.

:: Change to web project directory and run
cd /d "%WEB_PROJECT%"
dotnet run --launch-profile "http" --verbosity quiet

:: Pause on exit
echo.
echo [INFO] Application stopped
pause
