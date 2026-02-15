# Contact Manager - Local Development Guide

## Quick Start (No Docker Required)

### Windows

#### Option 1: Double-click (Easiest)
1. Open folder `CI-CD/`
2. Double-click `start-local.bat`
3. Wait for browser to open automatically at `http://localhost:5021`

#### Option 2: PowerShell
```powershell
.\CI-CD\start-local.ps1
```

#### Option 3: Command Prompt
```cmd
cd CI-CD
start-local.bat
```

### macOS / Linux

```bash
cd CI-CD
chmod +x start-local.sh
./start-local.sh
```

## Manual Start (Advanced)

If you prefer to start manually:

```bash
# Navigate to project root
cd ContactManager.Web

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or higher)
- Any modern web browser

## What Happens When You Run

1. **Restore** - Downloads all required NuGet packages
2. **Build** - Compiles the application
3. **Database** - Creates SQLite database automatically (first run)
4. **Server** - Starts Kestrel web server on port 5021
5. **Browser** - Opens `http://localhost:5021`

## Default Behavior

- **Port**: 5021
- **Database**: SQLite (auto-created)
- **Profile**: HTTP (not HTTPS, no certificate required)
- **Logging**: Minimal (quiet mode)

## Troubleshooting

### Port 5021 is already in use
Change port in `ContactManager.Web/Properties/launchSettings.json` or run:
```bash
dotnet run --urls "http://localhost:5001"
```

### .NET SDK not found
Install from: https://dotnet.microsoft.com/download

### Build errors
Try cleaning:
```bash
dotnet clean
dotnet restore
dotnet build
```

### Database locked
Delete the SQLite files and restart:
```bash
del ContactManager.db*
dotnet run
```

## Features Available

After starting, you can:
- Upload CSV files with contacts
- View all contacts in DataTable
- Filter and sort by any column
- Edit contacts inline
- Delete contacts
- Export data
- View API documentation at `/Documentation`

## Stop Application

Press `Ctrl+C` in the terminal window where the server is running.

## Alternative: Docker

For containerized run, see [DOCKER_MSSQL_GUIDE.md](DOCKER_MSSQL_GUIDE.md)
