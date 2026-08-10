#!/usr/bin/env pwsh
# Script para executar a API localmente sem Docker

param(
	[Parameter(Position=0)]
	[ValidateSet("run", "watch", "build", "test", "clean", "help")]
	[string]$Command = "run"
)

$ErrorActionPreference = "Stop"
$rootPath = Split-Path -Parent $PSScriptRoot

function Show-Banner {
	Write-Host "`n========================================" -ForegroundColor Cyan
	Write-Host "  Order Management API - Local Runner" -ForegroundColor Cyan
	Write-Host "========================================`n" -ForegroundColor Cyan
}

function Show-Help {
	Write-Host "Usage: .\run-local.ps1 [command]`n" -ForegroundColor Yellow
	Write-Host "Commands:" -ForegroundColor White
	Write-Host "  run     - Run the API (default)" -ForegroundColor Green
	Write-Host "  watch   - Run with hot reload (file watcher)" -ForegroundColor Green
	Write-Host "  build   - Build the solution" -ForegroundColor Green
	Write-Host "  test    - Run all tests" -ForegroundColor Green
	Write-Host "  clean   - Clean build artifacts" -ForegroundColor Green
	Write-Host "  help    - Show this help`n" -ForegroundColor Green

	Write-Host "Examples:" -ForegroundColor White
	Write-Host "  .\run-local.ps1           # Run the API" -ForegroundColor Gray
	Write-Host "  .\run-local.ps1 watch     # Run with auto-reload" -ForegroundColor Gray
	Write-Host "  .\run-local.ps1 test      # Run tests`n" -ForegroundColor Gray
}

function Run-Api {
	Show-Banner
	Write-Host "[INFO] Starting API..." -ForegroundColor Blue
	Write-Host "[INFO] Project: $rootPath\src\API\OrderManagement.API`n" -ForegroundColor Gray

	Set-Location "$rootPath\src\API\OrderManagement.API"

	Write-Host "========================================" -ForegroundColor Green
	Write-Host "  API will be available at:" -ForegroundColor Green
	Write-Host "  - http://localhost:5180" -ForegroundColor Yellow
	Write-Host "  - Swagger: http://localhost:5180/swagger" -ForegroundColor Yellow
	Write-Host "  - Health: http://localhost:5180/health" -ForegroundColor Yellow
	Write-Host "========================================`n" -ForegroundColor Green

	Write-Host "[INFO] Press Ctrl+C to stop the API`n" -ForegroundColor Gray

	dotnet run
}

function Run-Watch {
	Show-Banner
	Write-Host "[INFO] Starting API with hot reload..." -ForegroundColor Blue
	Write-Host "[INFO] Changes will be detected automatically`n" -ForegroundColor Gray

	Set-Location "$rootPath\src\API\OrderManagement.API"

	Write-Host "========================================" -ForegroundColor Green
	Write-Host "  API will be available at:" -ForegroundColor Green
	Write-Host "  - http://localhost:5180" -ForegroundColor Yellow
	Write-Host "  - Swagger: http://localhost:5180/swagger" -ForegroundColor Yellow
	Write-Host "========================================`n" -ForegroundColor Green

	dotnet watch run
}

function Run-Build {
	Show-Banner
	Write-Host "[INFO] Building solution..." -ForegroundColor Blue

	Set-Location $rootPath
	dotnet build --no-incremental

	if ($LASTEXITCODE -eq 0) {
		Write-Host "`n[SUCCESS] Build completed successfully!" -ForegroundColor Green
	} else {
		Write-Host "`n[ERROR] Build failed!" -ForegroundColor Red
		exit 1
	}
}

function Run-Tests {
	Show-Banner
	Write-Host "[INFO] Running all tests..." -ForegroundColor Blue

	Set-Location $rootPath

	Write-Host "`n--- Unit Tests ---" -ForegroundColor Cyan
	dotnet test tests/OrderManagement.UnitTests --no-build --verbosity minimal

	Write-Host "`n--- Integration Tests ---" -ForegroundColor Cyan
	dotnet test tests/OrderManagement.IntegrationTests --no-build --verbosity minimal

	if ($LASTEXITCODE -eq 0) {
		Write-Host "`n[SUCCESS] All tests passed!" -ForegroundColor Green
	} else {
		Write-Host "`n[ERROR] Some tests failed!" -ForegroundColor Red
		exit 1
	}
}

function Run-Clean {
	Show-Banner
	Write-Host "[INFO] Cleaning build artifacts..." -ForegroundColor Blue

	Set-Location $rootPath

	dotnet clean

	Write-Host "[INFO] Removing bin and obj directories..." -ForegroundColor Gray
	Get-ChildItem -Path $rootPath -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force

	Write-Host "[SUCCESS] Clean completed!" -ForegroundColor Green
}

# Main execution
try {
	switch ($Command) {
		"run"   { Run-Api }
		"watch" { Run-Watch }
		"build" { Run-Build }
		"test"  { Run-Tests }
		"clean" { Run-Clean }
		"help"  { Show-Help }
		default { Show-Help }
	}
} catch {
	Write-Host "`n[ERROR] $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}
