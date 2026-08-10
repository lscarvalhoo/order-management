#!/bin/bash

# SonarQube Local Analysis Script
# Runs SonarScanner without Docker

set -e

PROJECT_KEY="${1:-order-management}"
PROJECT_NAME="${2:-Order Management API}"
SONAR_URL="${3:-http://localhost:9000}"

GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

print_info() {
	echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
	echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
	echo -e "${RED}[ERROR]${NC} $1"
}

# Check if SONAR_TOKEN is set
if [ -z "$SONAR_TOKEN" ]; then
	print_error "SONAR_TOKEN environment variable is required"
	print_info "Set it with: export SONAR_TOKEN=\"your-token\""
	exit 1
fi

# Check if dotnet-sonarscanner is installed
if ! command -v dotnet-sonarscanner &> /dev/null; then
	print_error "dotnet-sonarscanner is not installed"
	print_info "Install it with: dotnet tool install --global dotnet-sonarscanner"
	exit 1
fi

print_info "Starting SonarQube analysis..."

# Begin analysis
print_info "Step 1/3: Begin SonarScanner"
dotnet sonarscanner begin \
	/k:"$PROJECT_KEY" \
	/n:"$PROJECT_NAME" \
	/d:sonar.host.url="$SONAR_URL" \
	/d:sonar.token="$SONAR_TOKEN" \
	/d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
	/d:sonar.exclusions="**/bin/**,**/obj/**,**/migrations/**,**/wwwroot/**"

# Build the solution
print_info "Step 2/3: Building solution"
dotnet build OrderManagement.sln --configuration Release --no-incremental

# Run tests with coverage (optional but recommended)
print_info "Running tests with coverage..."
dotnet test OrderManagement.sln \
	--configuration Release \
	--no-build \
	--collect:"XPlat Code Coverage" \
	--results-directory "./TestResults" \
	-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# End analysis
print_info "Step 3/3: Ending SonarScanner"
dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"

print_success "Analysis completed successfully!"
print_info "Check results at $SONAR_URL"
print_info "Project: $PROJECT_KEY"
