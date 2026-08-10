# SonarQube Helper Script for Order Management API (PowerShell)

param(
	[Parameter(Position=0)]
	[string]$Command = "help"
)

$ErrorActionPreference = "Stop"

# Determine script and project root directories
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$SonarComposeFile = Join-Path $ProjectRoot "build\docker-compose.sonar.yml"

function Write-Info {
	param([string]$Message)
	Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
	param([string]$Message)
	Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
	param([string]$Message)
	Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
	param([string]$Message)
	Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Show-Help {
	@"
Order Management API - SonarQube Helper

Usage: .\sonar.ps1 [command]

Commands:
	start       Start SonarQube and PostgreSQL
	stop        Stop SonarQube services
	restart     Restart SonarQube services
	logs        Show logs from SonarQube
	analyze     Run code analysis (requires SONAR_TOKEN)
	status      Check SonarQube status
	token       Show instructions to create a token
	clean       Stop services and remove volumes
	help        Show this help message

Environment Variables:
	SONAR_TOKEN     SonarQube authentication token (required for analysis)

Examples:
	.\sonar.ps1 start
	`$env:SONAR_TOKEN="your_token"; .\sonar.ps1 analyze
	.\sonar.ps1 logs
	.\sonar.ps1 status

"@
}

function Test-Docker {
	if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
		Write-Error "Docker is not installed."
		exit 1
	}

	if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
		$composeCheck = docker compose version 2>$null
		if (-not $composeCheck) {
			Write-Error "Docker Compose is not installed."
			exit 1
		}
	}
}

function Start-SonarQube {
	Write-Info "Starting SonarQube and PostgreSQL..."
	docker-compose -f $SonarComposeFile up -d sonarqube sonarqube-db
	Write-Success "SonarQube started successfully"
	Write-Info "SonarQube will be available at http://localhost:9000"
	Write-Info "Default credentials: admin/admin (you will be prompted to change)"
	Write-Warning "Wait ~60 seconds for SonarQube to fully start"
}

function Stop-SonarQube {
	Write-Info "Stopping SonarQube services..."
	docker-compose -f $SonarComposeFile down
	Write-Success "SonarQube stopped successfully"
}

function Restart-SonarQube {
	Write-Info "Restarting SonarQube services..."
	docker-compose -f $SonarComposeFile restart sonarqube sonarqube-db
	Write-Success "SonarQube restarted successfully"
}

function Show-Logs {
	Write-Info "Showing SonarQube logs (Ctrl+C to exit)..."
	docker-compose -f $SonarComposeFile logs -f sonarqube
}

function Start-Analysis {
	if (-not $env:SONAR_TOKEN) {
		Write-Error "SONAR_TOKEN environment variable is required"
		Write-Info "Run: .\sonar.ps1 token"
		exit 1
	}

	Write-Info "Starting code analysis..."
	docker-compose -f $SonarComposeFile --profile analysis up --build scanner
	Write-Success "Analysis completed! Check results at http://localhost:9000"
}

function Test-Status {
	Write-Info "Checking SonarQube status..."

	$containers = docker ps --filter "name=ordermanagement-sonarqube" --format "{{.Names}}"
	if (-not $containers) {
		Write-Error "SonarQube container is not running"
		Write-Info "Run: .\sonar.ps1 start"
		exit 1
	}

	try {
		$response = Invoke-WebRequest -Uri "http://localhost:9000/api/system/status" -UseBasicParsing -TimeoutSec 5
		if ($response.StatusCode -eq 200) {
			$status = ($response.Content | ConvertFrom-Json).status
			Write-Success "SonarQube is running (Status: $status)"
			Write-Info "Access: http://localhost:9000"
		}
	} catch {
		Write-Warning "SonarQube is starting... Please wait a few more seconds"
		Write-Info "Run: .\sonar.ps1 status (to check again)"
	}
}

function Show-TokenInstructions {
	@"
╔════════════════════════════════════════════════════════╗
║          How to Create a SonarQube Token              ║
╚════════════════════════════════════════════════════════╝

Step 1: Start SonarQube (if not already running)
	.\sonar.ps1 start

Step 2: Wait for SonarQube to start (~60 seconds)
	.\sonar.ps1 status

Step 3: Access SonarQube
	Open: http://localhost:9000
	Login: admin / admin
	(You'll be prompted to change the password)

Step 4: Create a Project
	1. Click "Create Project" → "Manually"
	2. Project key: order-management
	3. Display name: Order Management API
	4. Click "Set Up"

Step 5: Generate Token
	1. Choose "Locally"
	2. Generate a token
	3. Copy the token (you won't see it again!)

Step 6: Run analysis with your token
	`$env:SONAR_TOKEN="your_token_here"
	.\sonar.ps1 analyze

Alternative: Create token via API
	`$cred = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:your_password"))
	Invoke-RestMethod -Uri "http://localhost:9000/api/user_tokens/generate?name=scanner" ``
		-Method Post ``
		-Headers @{Authorization="Basic `$cred"}

"@
}

function Remove-Volumes {
	Write-Warning "This will remove all SonarQube data and analysis history!"
	$response = Read-Host "Are you sure? (y/N)"

	if ($response -match "^[Yy]") {
		Write-Info "Stopping and removing SonarQube volumes..."
		docker-compose -f docker-compose.sonar.yml down -v
		Write-Success "Cleanup completed"
	} else {
		Write-Info "Cleanup cancelled"
	}
}

# Main script logic
Test-Docker

switch ($Command.ToLower()) {
	"start" {
		Start-SonarQube
	}
	"stop" {
		Stop-SonarQube
	}
	"restart" {
		Restart-SonarQube
	}
	"logs" {
		Show-Logs
	}
	"analyze" {
		Start-Analysis
	}
	"status" {
		Test-Status
	}
	"token" {
		Show-TokenInstructions
	}
	"clean" {
		Remove-Volumes
	}
	"help" {
		Show-Help
	}
	default {
		Write-Error "Unknown command: $Command"
		Write-Host ""
		Show-Help
		exit 1
	}
}
