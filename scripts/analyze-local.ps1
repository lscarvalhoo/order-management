# SonarQube Local Analysis Script
# Runs SonarScanner without Docker

param(
	[string]$ProjectKey = "order-management",
	[string]$ProjectName = "Order Management API",
	[string]$SonarUrl = "http://localhost:9000"
)

$ErrorActionPreference = "Stop"

function Write-Info {
	param([string]$Message)
	Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
	param([string]$Message)
	Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Error {
	param([string]$Message)
	Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check if SONAR_TOKEN is set
if (-not $env:SONAR_TOKEN) {
	Write-Error "SONAR_TOKEN environment variable is required"
	Write-Info "Set it with: `$env:SONAR_TOKEN=`"your-token`""
	exit 1
}

# Check if dotnet-sonarscanner is installed
if (-not (Get-Command dotnet-sonarscanner -ErrorAction SilentlyContinue)) {
	Write-Error "dotnet-sonarscanner is not installed"
	Write-Info "Install it with: dotnet tool install --global dotnet-sonarscanner"
	exit 1
}

Write-Info "Starting SonarQube analysis..."

# Begin analysis
Write-Info "Step 1/3: Begin SonarScanner"
dotnet sonarscanner begin `
	/k:"$ProjectKey" `
	/n:"$ProjectName" `
	/d:sonar.host.url="$SonarUrl" `
	/d:sonar.token="$env:SONAR_TOKEN" `
	/d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" `
	/d:sonar.exclusions="**/bin/**,**/obj/**,**/migrations/**,**/wwwroot/**"

if ($LASTEXITCODE -ne 0) {
	Write-Error "Failed to begin SonarScanner"
	exit $LASTEXITCODE
}

# Build the solution
Write-Info "Step 2/3: Building solution"
dotnet build OrderManagement.sln --configuration Release --no-incremental

if ($LASTEXITCODE -ne 0) {
	Write-Error "Build failed"
	exit $LASTEXITCODE
}

# Run tests with coverage (optional but recommended)
Write-Info "Running tests with coverage..."
dotnet test OrderManagement.sln `
	--configuration Release `
	--no-build `
	--collect:"XPlat Code Coverage" `
	--results-directory "./TestResults" `
	-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# End analysis
Write-Info "Step 3/3: Ending SonarScanner"
dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"

if ($LASTEXITCODE -ne 0) {
	Write-Error "Failed to end SonarScanner"
	exit $LASTEXITCODE
}

Write-Success "Analysis completed successfully!"
Write-Info "Check results at $SonarUrl"
Write-Info "Project: $ProjectKey"
