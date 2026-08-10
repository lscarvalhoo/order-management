#!/bin/bash

# SonarQube Helper Script for Order Management API

set -e

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_info() {
	echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
	echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
	echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
	echo -e "${RED}[ERROR]${NC} $1"
}

show_help() {
	cat << EOF
Order Management API - SonarQube Helper

Usage: ./sonar.sh [command]

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
	./sonar.sh start
	SONAR_TOKEN=your_token ./sonar.sh analyze
	./sonar.sh logs
	./sonar.sh status

EOF
}

check_docker() {
	if ! command -v docker &> /dev/null; then
		print_error "Docker is not installed."
		exit 1
	fi

	if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
		print_error "Docker Compose is not installed."
		exit 1
	fi
}

start_sonarqube() {
	print_info "Starting SonarQube and PostgreSQL..."
	docker-compose -f docker-compose.sonar.yml up -d sonarqube sonarqube-db
	print_success "SonarQube started successfully"
	print_info "SonarQube will be available at http://localhost:9000"
	print_info "Default credentials: admin/admin (you will be prompted to change)"
	print_warning "Wait ~60 seconds for SonarQube to fully start"
}

stop_sonarqube() {
	print_info "Stopping SonarQube services..."
	docker-compose -f docker-compose.sonar.yml down
	print_success "SonarQube stopped successfully"
}

restart_sonarqube() {
	print_info "Restarting SonarQube services..."
	docker-compose -f docker-compose.sonar.yml restart sonarqube sonarqube-db
	print_success "SonarQube restarted successfully"
}

show_logs() {
	print_info "Showing SonarQube logs (Ctrl+C to exit)..."
	docker-compose -f docker-compose.sonar.yml logs -f sonarqube
}

run_analysis() {
	if [ -z "$SONAR_TOKEN" ]; then
		print_error "SONAR_TOKEN environment variable is required"
		print_info "Run: ./sonar.sh token"
		exit 1
	fi

	print_info "Starting code analysis..."
	docker-compose -f docker-compose.sonar.yml --profile analysis up --build scanner
	print_success "Analysis completed! Check results at http://localhost:9000"
}

check_status() {
	print_info "Checking SonarQube status..."

	if ! docker ps | grep -q ordermanagement-sonarqube; then
		print_error "SonarQube container is not running"
		print_info "Run: ./sonar.sh start"
		exit 1
	fi

	response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:9000/api/system/status)

	if [ "$response" = "200" ]; then
		status=$(curl -s http://localhost:9000/api/system/status | grep -o '"status":"[^"]*' | cut -d'"' -f4)
		print_success "SonarQube is running (Status: $status)"
		print_info "Access: http://localhost:9000"
	else
		print_warning "SonarQube is starting... (HTTP $response)"
		print_info "Wait a few more seconds and try again"
	fi
}

show_token_instructions() {
	cat << EOF
${BLUE}╔════════════════════════════════════════════════════════╗${NC}
${BLUE}║          How to Create a SonarQube Token              ║${NC}
${BLUE}╚════════════════════════════════════════════════════════╝${NC}

${YELLOW}Step 1:${NC} Start SonarQube (if not already running)
	./sonar.sh start

${YELLOW}Step 2:${NC} Wait for SonarQube to start (~60 seconds)
	./sonar.sh status

${YELLOW}Step 3:${NC} Access SonarQube
	Open: http://localhost:9000
	Login: admin / admin
	(You'll be prompted to change the password)

${YELLOW}Step 4:${NC} Create a Project
	1. Click "Create Project" → "Manually"
	2. Project key: order-management
	3. Display name: Order Management API
	4. Click "Set Up"

${YELLOW}Step 5:${NC} Generate Token
	1. Choose "Locally"
	2. Generate a token
	3. Copy the token (you won't see it again!)

${YELLOW}Step 6:${NC} Run analysis with your token
	SONAR_TOKEN=your_token_here ./sonar.sh analyze

${GREEN}Alternative:${NC} Create token via API
	curl -u admin:your_password \\
	  -X POST "http://localhost:9000/api/user_tokens/generate?name=scanner"

EOF
}

clean_volumes() {
	print_warning "This will remove all SonarQube data and analysis history!"
	read -p "Are you sure? (y/N) " -n 1 -r
	echo
	if [[ $REPLY =~ ^[Yy]$ ]]; then
		print_info "Stopping and removing SonarQube volumes..."
		docker-compose -f docker-compose.sonar.yml down -v
		print_success "Cleanup completed"
	else
		print_info "Cleanup cancelled"
	fi
}

# Main script logic
check_docker

case "${1:-}" in
	start)
		start_sonarqube
		;;
	stop)
		stop_sonarqube
		;;
	restart)
		restart_sonarqube
		;;
	logs)
		show_logs
		;;
	analyze)
		run_analysis
		;;
	status)
		check_status
		;;
	token)
		show_token_instructions
		;;
	clean)
		clean_volumes
		;;
	help|--help|-h)
		show_help
		;;
	*)
		print_error "Unknown command: ${1:-}"
		echo ""
		show_help
		exit 1
		;;
esac
