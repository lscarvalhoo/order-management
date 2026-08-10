# Docker Setup Validation Script (PowerShell)
# This script validates that the Docker setup is working correctly

$ErrorActionPreference = "Stop"

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║   Order Management API - Docker Validation Script     ║" -ForegroundColor Blue
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Blue
Write-Host ""

function Test-Docker {
	Write-Host "Checking Docker installation... " -NoNewline
	if (Get-Command docker -ErrorAction SilentlyContinue) {
		Write-Host "OK" -ForegroundColor Green
		docker --version
	} else {
		Write-Host "ERROR" -ForegroundColor Red
		Write-Host "Docker is not installed!" -ForegroundColor Red
		exit 1
	}
	Write-Host ""
}

function Test-DockerCompose {
	Write-Host "Checking Docker Compose installation... " -NoNewline
	if (Get-Command docker-compose -ErrorAction SilentlyContinue) {
		Write-Host "OK" -ForegroundColor Green
		docker-compose --version
	} else {
		$composeCheck = docker compose version 2>$null
		if ($composeCheck) {
			Write-Host "OK" -ForegroundColor Green
			docker compose version
		} else {
			Write-Host "ERROR" -ForegroundColor Red
			Write-Host "Docker Compose is not installed!" -ForegroundColor Red
			exit 1
		}
	}
	Write-Host ""
}

function Test-RequiredFiles {
	Write-Host "Checking required files..."

	$files = @(
		"Dockerfile",
		".dockerignore",
		"docker-compose.yml",
		".env.example",
		"docker.sh",
		"docker.ps1",
		"docs/DOCKER.md"
	)

	$allFound = $true
	foreach ($file in $files) {
		Write-Host "  - $file... " -NoNewline
		if (Test-Path $file) {
			Write-Host "OK" -ForegroundColor Green
		} else {
			Write-Host "ERROR" -ForegroundColor Red
			$allFound = $false
		}
	}

	Write-Host ""
	if (-not $allFound) {
		Write-Host "Some required files are missing!" -ForegroundColor Red
		exit 1
	}
}

function Build-DockerImage {
	Write-Host "Building Docker image..." -ForegroundColor Yellow
	docker-compose build --no-cache
	Write-Host "OK Build successful" -ForegroundColor Green
	Write-Host ""
}

function Start-DockerServices {
	Write-Host "Starting services..." -ForegroundColor Yellow
	New-Item -ItemType Directory -Force -Path "data" | Out-Null
	New-Item -ItemType Directory -Force -Path "logs" | Out-Null
	docker-compose up -d
	Write-Host "OK Services started" -ForegroundColor Green
	Write-Host ""
}

function Wait-ForHealth {
	Write-Host "Waiting for API to be healthy" -NoNewline
	$maxAttempts = 30
	$attempt = 0

	while ($attempt -lt $maxAttempts) {
		try {
			$response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
			if ($response.StatusCode -eq 200) {
				Write-Host " OK" -ForegroundColor Green
				return $true
			}
		} catch {
			Write-Host "." -NoNewline
			Start-Sleep -Seconds 2
			$attempt++
		}
	}

	Write-Host " ERROR" -ForegroundColor Red
	Write-Host "API failed to become healthy after $maxAttempts attempts" -ForegroundColor Red
	docker-compose logs api
	return $false
}

function Test-Endpoints {
	Write-Host "Testing API endpoints..." -ForegroundColor Yellow

	# Test health endpoint
	Write-Host "  - Health check... " -NoNewline
	try {
		$response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing
		if ($response.StatusCode -eq 200) {
			Write-Host "OK" -ForegroundColor Green
		}
	} catch {
		Write-Host "ERROR" -ForegroundColor Red
		return $false
	}

	# Test Swagger
	Write-Host "  - Swagger UI... " -NoNewline
	try {
		$response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -UseBasicParsing
		if ($response.StatusCode -eq 200) {
			Write-Host "OK" -ForegroundColor Green
		}
	} catch {
		Write-Host "ERROR" -ForegroundColor Red
		return $false
	}

	# Test login endpoint
	Write-Host "  - Login endpoint... " -NoNewline
	try {
		$body = @{
			email = "admin@ordermanagement.com"
			password = "Admin@123"
		} | ConvertTo-Json

		$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
			-Method Post `
			-Body $body `
			-ContentType "application/json"

		if ($response.token) {
			Write-Host "OK" -ForegroundColor Green
			$token = $response.token
			Write-Host "    Token received: $($token.Substring(0, [Math]::Min(50, $token.Length)))..."
		}
	} catch {
		Write-Host "ERROR" -ForegroundColor Red
		Write-Host $_.Exception.Message
		return $false
	}

	# Test create order
	Write-Host "  - Create order endpoint... " -NoNewline
	try {
		$orderBody = @{
			customerId = "550e8400-e29b-41d4-a716-446655440000"
			items = @(
				@{
					productName = "Test Product"
					quantity = 1
					unitPrice = 100.00
				}
			)
		} | ConvertTo-Json

		$headers = @{
			Authorization = "Bearer $token"
		}

		$response = Invoke-RestMethod -Uri "http://localhost:5000/api/orders" `
			-Method Post `
			-Body $orderBody `
			-ContentType "application/json" `
			-Headers $headers

		if ($response.id) {
			Write-Host "OK" -ForegroundColor Green
			Write-Host "    Order created: $($response.id)"
		}
	} catch {
		Write-Host "ERROR" -ForegroundColor Red
		Write-Host $_.Exception.Message
		return $false
	}

	Write-Host ""
	return $true
}

function Test-Volumes {
	Write-Host "Checking volumes..." -ForegroundColor Yellow

	Write-Host "  - Database file... " -NoNewline
	if (Test-Path "data/ordermanagement.db") {
		Write-Host "OK" -ForegroundColor Green
		Get-Item "data/ordermanagement.db" | Format-Table Name, Length, LastWriteTime
	} else {
		Write-Host "ERROR" -ForegroundColor Red
	}

	Write-Host "  - Log files... " -NoNewline
	if ((Test-Path "logs") -and (Get-ChildItem "logs").Count -gt 0) {
		Write-Host "OK" -ForegroundColor Green
		Get-ChildItem "logs" | Format-Table Name, Length, LastWriteTime
	} else {
		Write-Host "WARNING (no logs yet)" -ForegroundColor Yellow
	}

	Write-Host ""
}

function Test-Logs {
	Write-Host "Checking logs for errors..." -ForegroundColor Yellow

	$logs = docker-compose logs api
	$errors = $logs | Select-String -Pattern "error" -SimpleMatch

	if ($errors) {
		Write-Host "WARNING: Found error messages in logs:" -ForegroundColor Yellow
		$errors | ForEach-Object { Write-Host $_.Line }
	} else {
		Write-Host "OK No errors found in logs" -ForegroundColor Green
	}

	Write-Host ""
}

function Show-Summary {
	Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Blue
	Write-Host "║              Validation Summary                        ║" -ForegroundColor Blue
	Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Blue
	Write-Host ""
	Write-Host "OK All checks passed successfully!" -ForegroundColor Green
	Write-Host ""
	Write-Host "Service URLs:"
	Write-Host "  - API:     http://localhost:5000"
	Write-Host "  - Swagger: http://localhost:5000/swagger"
	Write-Host "  - Health:  http://localhost:5000/health"
	Write-Host ""
	Write-Host "Useful commands:"
	Write-Host "  - View logs:    docker-compose logs -f api"
	Write-Host "  - Stop:         docker-compose down"
	Write-Host "  - Restart:      docker-compose restart"
	Write-Host "  - Shell:        docker-compose exec api /bin/bash"
	Write-Host ""
}

function Stop-DockerServices {
	Write-Host "Cleaning up..." -ForegroundColor Yellow
	docker-compose down
	Write-Host "OK Cleanup complete" -ForegroundColor Green
	Write-Host ""
}

# Main execution
Test-Docker
Test-DockerCompose
Test-RequiredFiles

$response = Read-Host "Do you want to build and test the Docker setup? (y/n)"

if ($response -match "^[Yy]") {
	Build-DockerImage
	Start-DockerServices

	if (Wait-ForHealth) {
		if (Test-Endpoints) {
			Test-Volumes
			Test-Logs
			Show-Summary
		}
	} else {
		Write-Host "Health check failed. Check logs for details:" -ForegroundColor Red
		docker-compose logs api
		exit 1
	}

	$stopResponse = Read-Host "Do you want to stop the services? (y/n)"

	if ($stopResponse -match "^[Yy]") {
		Stop-DockerServices
	} else {
		Write-Host "Services are still running. Use 'docker-compose down' to stop them." -ForegroundColor Green
	}
} else {
	Write-Host "Validation skipped. Only file checks were performed."
}

Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Blue
Write-Host "Validation script completed!" -ForegroundColor Blue
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Blue
