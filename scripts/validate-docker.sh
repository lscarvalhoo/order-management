#!/bin/bash

# Docker Setup Validation Script
# This script validates that the Docker setup is working correctly

set -e

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   Order Management API - Docker Validation Script     ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check functions
check_docker() {
	echo -n "Checking Docker installation... "
	if command -v docker &> /dev/null; then
		echo -e "${GREEN}OK${NC}"
		docker --version
	else
		echo -e "${RED}ERROR${NC}"
		echo "Docker is not installed!"
		exit 1
	fi
	echo ""
}

check_docker_compose() {
	echo -n "Checking Docker Compose installation... "
	if command -v docker-compose &> /dev/null || docker compose version &> /dev/null; then
		echo -e "${GREEN}OK${NC}"
		docker-compose --version 2>/dev/null || docker compose version
	else
		echo -e "${RED}ERROR${NC}"
		echo "Docker Compose is not installed!"
		exit 1
	fi
	echo ""
}

check_files() {
	echo "Checking required files..."

	files=(
		"Dockerfile"
		".dockerignore"
		"docker-compose.yml"
		".env.example"
		"docker.sh"
		"docker.ps1"
		"docs/DOCKER.md"
	)

	all_found=true
	for file in "${files[@]}"; do
		echo -n "  - $file... "
		if [ -f "$file" ]; then
			echo -e "${GREEN}OK${NC}"
		else
			echo -e "${RED}ERROR${NC}"
			all_found=false
		fi
	done

	echo ""
	if [ "$all_found" = false ]; then
		echo -e "${RED}Some required files are missing!${NC}"
		exit 1
	fi
}

build_image() {
	echo -e "${YELLOW}Building Docker image...${NC}"
	docker-compose build --no-cache
	echo -e "${GREEN}OK Build successful${NC}"
	echo ""
}

start_services() {
	echo -e "${YELLOW}Starting services...${NC}"
	mkdir -p data logs
	docker-compose up -d
	echo -e "${GREEN}OK Services started${NC}"
	echo ""
}

wait_for_health() {
	echo -n "Waiting for API to be healthy"
	max_attempts=30
	attempt=0

	while [ $attempt -lt $max_attempts ]; do
		if curl -s -f http://localhost:5000/health > /dev/null 2>&1; then
			echo -e " ${GREEN}OK${NC}"
			return 0
		fi
		echo -n "."
		sleep 2
		attempt=$((attempt + 1))
	done

	echo -e " ${RED}ERROR${NC}"
	echo "API failed to become healthy after $max_attempts attempts"
	docker-compose logs api
	return 1
}

test_endpoints() {
	echo -e "${YELLOW}Testing API endpoints...${NC}"

	# Test health endpoint
	echo -n "  - Health check... "
	if curl -s -f http://localhost:5000/health > /dev/null; then
		echo -e "${GREEN}OK${NC}"
	else
		echo -e "${RED}ERROR${NC}"
		return 1
	fi

	# Test Swagger
	echo -n "  - Swagger UI... "
	if curl -s -f http://localhost:5000/swagger/index.html > /dev/null; then
		echo -e "${GREEN}OK${NC}"
	else
		echo -e "${RED}ERROR${NC}"
		return 1
	fi

	# Test login endpoint
	echo -n "  - Login endpoint... "
	response=$(curl -s -X POST http://localhost:5000/api/auth/login \
		-H "Content-Type: application/json" \
		-d '{"email":"admin@ordermanagement.com","password":"Admin@123"}' \
		-w "%{http_code}" -o /tmp/login_response.json)

	if [ "$response" = "200" ]; then
		echo -e "${GREEN}OK${NC}"
		token=$(cat /tmp/login_response.json | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
		echo "    Token received: ${token:0:50}..."
	else
		echo -e "${RED}ERROR${NC}"
		cat /tmp/login_response.json
		return 1
	fi

	# Test create order
	echo -n "  - Create order endpoint... "
	response=$(curl -s -X POST http://localhost:5000/api/orders \
		-H "Content-Type: application/json" \
		-H "Authorization: Bearer $token" \
		-d '{
			"customerId": "550e8400-e29b-41d4-a716-446655440000",
			"items": [
				{
					"productName": "Test Product",
					"quantity": 1,
					"unitPrice": 100.00
				}
			]
		}' \
		-w "%{http_code}" -o /tmp/create_order_response.json)

	if [ "$response" = "201" ]; then
		echo -e "${GREEN}OK${NC}"
		order_id=$(cat /tmp/create_order_response.json | grep -o '"id":"[^"]*' | grep -o '[^"]*$')
		echo "    Order created: $order_id"
	else
		echo -e "${RED}ERROR${NC}"
		cat /tmp/create_order_response.json
		return 1
	fi

	echo ""
}

check_volumes() {
	echo -e "${YELLOW}Checking volumes...${NC}"

	echo -n "  - Database file... "
	if [ -f "data/ordermanagement.db" ]; then
		echo -e "${GREEN}OK${NC}"
		ls -lh data/ordermanagement.db
	else
		echo -e "${RED}ERROR${NC}"
	fi

	echo -n "  - Log files... "
	if [ -d "logs" ] && [ "$(ls -A logs)" ]; then
		echo -e "${GREEN}OK${NC}"
		ls -lh logs/
	else
		echo -e "${YELLOW}WARNING${NC} (no logs yet)"
	fi

	echo ""
}

check_logs() {
	echo -e "${YELLOW}Checking logs for errors...${NC}"

	if docker-compose logs api | grep -i "error" > /tmp/errors.txt; then
		echo -e "${YELLOW}WARNING: Found error messages in logs:${NC}"
		cat /tmp/errors.txt
	else
		echo -e "${GREEN}OK No errors found in logs${NC}"
	fi

	echo ""
}

show_summary() {
	echo -e "${BLUE}╔════════════════════════════════════════════════════════╗${NC}"
	echo -e "${BLUE}║              Validation Summary                        ║${NC}"
	echo -e "${BLUE}╚════════════════════════════════════════════════════════╝${NC}"
	echo ""
	echo -e "${GREEN}OK All checks passed successfully!${NC}"
	echo ""
	echo "Service URLs:"
	echo "  - API:     http://localhost:5000"
	echo "  - Swagger: http://localhost:5000/swagger"
	echo "  - Health:  http://localhost:5000/health"
	echo ""
	echo "Useful commands:"
	echo "  - View logs:    docker-compose logs -f api"
	echo "  - Stop:         docker-compose down"
	echo "  - Restart:      docker-compose restart"
	echo "  - Shell:        docker-compose exec api /bin/bash"
	echo ""
}

cleanup() {
	echo -e "${YELLOW}Cleaning up...${NC}"
	docker-compose down
	echo -e "${GREEN}OK Cleanup complete${NC}"
	echo ""
}

# Main execution
main() {
	check_docker
	check_docker_compose
	check_files

	echo -e "${YELLOW}Do you want to build and test the Docker setup? (y/n)${NC}"
	read -r response

	if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
		build_image
		start_services

		if wait_for_health; then
			test_endpoints
			check_volumes
			check_logs
			show_summary
		else
			echo -e "${RED}Health check failed. Check logs for details:${NC}"
			docker-compose logs api
			exit 1
		fi

		echo -e "${YELLOW}Do you want to stop the services? (y/n)${NC}"
		read -r stop_response

		if [[ "$stop_response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
			cleanup
		else
			echo -e "${GREEN}Services are still running. Use 'docker-compose down' to stop them.${NC}"
		fi
	else
		echo "Validation skipped. Only file checks were performed."
	fi
}

# Run main function
main

echo -e "${BLUE}════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}Validation script completed!${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════${NC}"
