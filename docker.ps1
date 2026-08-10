# Docker Helper Script for Order Management API (PowerShell)

param(
	[Parameter(Position=0)]
	[string]$Command = "help"
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
Order Management API - Docker Compose Helper

Usage: .\docker.ps1 [command]

Commands:
	build       Build the Docker image
	up          Start all services
	down        Stop all services
	restart     Restart all services
	logs        Show logs from all services
	logs-api    Show logs from API service only
	clean       Stop services and remove volumes
	rebuild     Clean rebuild (down, clean, build, up)
	health      Check API health status
	shell       Open a shell in the API container
	help        Show this help message

Examples:
	.\docker.ps1 build
	.\docker.ps1 up
	.\docker.ps1 logs-api
	.\docker.ps1 health

"@
}

function Test-Docker {
	if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
		Write-Error "Docker is not installed. Please install Docker first."
		exit 1
	}

	if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
		$composeCheck = docker compose version 2>$null
		if (-not $composeCheck) {
			Write-Error "Docker Compose is not installed. Please install Docker Compose first."
			exit 1
		}
	}
}

function New-Directories {
	Write-Info "Creating necessary directories..."
	New-Item -ItemType Directory -Force -Path "data" | Out-Null
	New-Item -ItemType Directory -Force -Path "logs" | Out-Null
	Write-Success "Directories created"
}

function Build-Image {
	Write-Info "Building Docker image..."
	docker-compose build --no-cache
	Write-Success "Docker image built successfully"
}

function Start-Services {
	Write-Info "Starting services..."
	New-Directories
	docker-compose up -d
	Write-Success "Services started successfully"
	Write-Info "API is running at http://localhost:5000"
	Write-Info "Swagger UI is available at http://localhost:5000/swagger"
	Write-Info "Use '.\docker.ps1 logs' to view logs"
}

function Stop-Services {
	Write-Info "Stopping services..."
	docker-compose down
	Write-Success "Services stopped successfully"
}

function Restart-Services {
	Write-Info "Restarting services..."
	docker-compose restart
	Write-Success "Services restarted successfully"
}

function Show-Logs {
	Write-Info "Showing logs (Ctrl+C to exit)..."
	docker-compose logs -f
}

function Show-ApiLogs {
	Write-Info "Showing API logs (Ctrl+C to exit)..."
	docker-compose logs -f api
}

function Remove-Volumes {
	Write-Info "Stopping services and removing volumes..."
	docker-compose down -v
	Write-Warning "Removing data and logs directories..."
	if (Test-Path "data") { Remove-Item -Path "data" -Recurse -Force }
	if (Test-Path "logs") { Remove-Item -Path "logs" -Recurse -Force }
	Write-Success "Cleanup completed"
}

function Rebuild-All {
	Write-Info "Starting full rebuild..."
	Stop-Services
	Remove-Volumes
	Build-Image
	Start-Services
	Write-Success "Rebuild completed successfully"
}

function Test-Health {
	Write-Info "Checking API health..."

	$containers = docker ps --filter "name=ordermanagement-api" --format "{{.Names}}"
	if (-not $containers) {
		Write-Error "API container is not running"
		exit 1
	}

	try {
		$response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
		if ($response.StatusCode -eq 200) {
			Write-Success "API is healthy (HTTP $($response.StatusCode))"
		} else {
			Write-Error "API health check failed (HTTP $($response.StatusCode))"
			exit 1
		}
	} catch {
		Write-Error "API health check failed: $($_.Exception.Message)"
		exit 1
	}
}

function Open-Shell {
	Write-Info "Opening shell in API container..."
	docker-compose exec api /bin/bash
}

# Main script logic
Test-Docker

switch ($Command.ToLower()) {
	"build" {
		Build-Image
	}
	"up" {
		Start-Services
	}
	"down" {
		Stop-Services
	}
	"restart" {
		Restart-Services
	}
	"logs" {
		Show-Logs
	}
	"logs-api" {
		Show-ApiLogs
	}
	"clean" {
		Remove-Volumes
	}
	"rebuild" {
		Rebuild-All
	}
	"health" {
		Test-Health
	}
	"shell" {
		Open-Shell
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
