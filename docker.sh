#!/bin/bash

# Docker Compose Helper Script for Order Management API

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

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
Order Management API - Docker Compose Helper

Usage: ./docker.sh [command]

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
	./docker.sh build
	./docker.sh up
	./docker.sh logs-api
	./docker.sh health

EOF
}

check_docker() {
	if ! command -v docker &> /dev/null; then
		print_error "Docker is not installed. Please install Docker first."
		exit 1
	fi

	if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
		print_error "Docker Compose is not installed. Please install Docker Compose first."
		exit 1
	fi
}

create_directories() {
	print_info "Creating necessary directories..."
	mkdir -p data logs
	print_success "Directories created"
}

build_image() {
	print_info "Building Docker image..."
	docker-compose build --no-cache
	print_success "Docker image built successfully"
}

start_services() {
	print_info "Starting services..."
	create_directories
	docker-compose up -d
	print_success "Services started successfully"
	print_info "API is running at http://localhost:5000"
	print_info "Swagger UI is available at http://localhost:5000/swagger"
	print_info "Use './docker.sh logs' to view logs"
}

stop_services() {
	print_info "Stopping services..."
	docker-compose down
	print_success "Services stopped successfully"
}

restart_services() {
	print_info "Restarting services..."
	docker-compose restart
	print_success "Services restarted successfully"
}

show_logs() {
	print_info "Showing logs (Ctrl+C to exit)..."
	docker-compose logs -f
}

show_api_logs() {
	print_info "Showing API logs (Ctrl+C to exit)..."
	docker-compose logs -f api
}

clean_volumes() {
	print_info "Stopping services and removing volumes..."
	docker-compose down -v
	print_warning "Removing data and logs directories..."
	rm -rf data logs
	print_success "Cleanup completed"
}

rebuild_all() {
	print_info "Starting full rebuild..."
	stop_services
	clean_volumes
	build_image
	start_services
	print_success "Rebuild completed successfully"
}

check_health() {
	print_info "Checking API health..."

	if ! docker ps | grep -q ordermanagement-api; then
		print_error "API container is not running"
		exit 1
	fi

	response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health)

	if [ "$response" = "200" ]; then
		print_success "API is healthy (HTTP $response)"
	else
		print_error "API health check failed (HTTP $response)"
		exit 1
	fi
}

open_shell() {
	print_info "Opening shell in API container..."
	docker-compose exec api /bin/bash
}

# Main script logic
check_docker

case "${1:-}" in
	build)
		build_image
		;;
	up)
		start_services
		;;
	down)
		stop_services
		;;
	restart)
		restart_services
		;;
	logs)
		show_logs
		;;
	logs-api)
		show_api_logs
		;;
	clean)
		clean_volumes
		;;
	rebuild)
		rebuild_all
		;;
	health)
		check_health
		;;
	shell)
		open_shell
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
