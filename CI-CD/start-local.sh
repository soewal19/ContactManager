#!/bin/bash
# Start Contact Manager Application locally
# Usage: ./start-local.sh

echo "=========================================="
echo "Contact Manager - Local Startup"
echo "=========================================="

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed${NC}"
    echo "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version | cut -d. -f1)
if [ "$DOTNET_VERSION" -lt 8 ]; then
    echo -e "${RED}Error: .NET 8 or higher is required${NC}"
    echo "Current version: $(dotnet --version)"
    exit 1
fi

echo -e "${GREEN}✓ .NET SDK found: $(dotnet --version)${NC}"

# Get project root (parent directory)
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WEB_PROJECT="$PROJECT_ROOT/ContactManager.Web"

echo ""
echo "Project root: $PROJECT_ROOT"
echo "Web project: $WEB_PROJECT"

# Restore dependencies
echo ""
echo -e "${YELLOW}→ Restoring NuGet packages...${NC}"
dotnet restore "$PROJECT_ROOT/ContactManager.sln" --verbosity quiet
if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Failed to restore packages${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Packages restored${NC}"

# Build project
echo ""
echo -e "${YELLOW}→ Building project...${NC}"
dotnet build "$PROJECT_ROOT/ContactManager.sln" --verbosity quiet
if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Build failed${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Build successful${NC}"

# Run application
echo ""
echo "=========================================="
echo -e "${GREEN}✓ Starting Contact Manager...${NC}"
echo "=========================================="
echo ""
echo -e "${YELLOW}Application will be available at:${NC}"
echo -e "  ${GREEN}http://localhost:5021${NC}"
echo ""
echo -e "Press ${YELLOW}Ctrl+C${NC} to stop the server"
echo ""

# Run the application
cd "$WEB_PROJECT"
dotnet run --launch-profile "http" --verbosity quiet

# Cleanup on exit
echo ""
echo -e "${YELLOW}Application stopped${NC}"
