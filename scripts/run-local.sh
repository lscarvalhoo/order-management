#!/bin/bash
# Script para executar a API localmente sem Docker

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_PATH="$(dirname "$SCRIPT_DIR")"
COMMAND="${1:-run}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

show_banner() {
	echo -e "${CYAN}"
	echo "========================================"
	echo "  Order Management API - Local Runner"
	echo "========================================"
	echo -e "${NC}"
}

show_help() {
	echo -e "${YELLOW}Usage: ./run-local.sh [command]${NC}\n"
	echo -e "${NC}Commands:${NC}"
	echo -e "${GREEN}  run     - Run the API (default)${NC}"
	echo -e "${GREEN}  watch   - Run with hot reload (file watcher)${NC}"
	echo -e "${GREEN}  build   - Build the solution${NC}"
	echo -e "${GREEN}  test    - Run all tests${NC}"
	echo -e "${GREEN}  clean   - Clean build artifacts${NC}"
	echo -e "${GREEN}  help    - Show this help${NC}\n"

	echo -e "${NC}Examples:${NC}"
	echo -e "${GRAY}  ./run-local.sh           # Run the API${NC}"
	echo -e "${GRAY}  ./run-local.sh watch     # Run with auto-reload${NC}"
	echo -e "${GRAY}  ./run-local.sh test      # Run tests${NC}\n"
}

run_api() {
	show_banner
	echo -e "${BLUE}[INFO] Starting API...${NC}"
	echo -e "${GRAY}[INFO] Project: $ROOT_PATH/src/API/OrderManagement.API${NC}\n"

	cd "$ROOT_PATH/src/API/OrderManagement.API"

	echo -e "${GREEN}========================================${NC}"
	echo -e "${GREEN}  API will be available at:${NC}"
	echo -e "${YELLOW}  - http://localhost:5180${NC}"
	echo -e "${YELLOW}  - Swagger: http://localhost:5180/swagger${NC}"
	echo -e "${YELLOW}  - Health: http://localhost:5180/health${NC}"
	echo -e "${GREEN}========================================${NC}\n"

	echo -e "${GRAY}[INFO] Press Ctrl+C to stop the API${NC}\n"

	dotnet run
}

run_watch() {
	show_banner
	echo -e "${BLUE}[INFO] Starting API with hot reload...${NC}"
	echo -e "${GRAY}[INFO] Changes will be detected automatically${NC}\n"

	cd "$ROOT_PATH/src/API/OrderManagement.API"

	echo -e "${GREEN}========================================${NC}"
	echo -e "${GREEN}  API will be available at:${NC}"
	echo -e "${YELLOW}  - http://localhost:5180${NC}"
	echo -e "${YELLOW}  - Swagger: http://localhost:5180/swagger${NC}"
	echo -e "${GREEN}========================================${NC}\n"

	dotnet watch run
}

run_build() {
	show_banner
	echo -e "${BLUE}[INFO] Building solution...${NC}"

	cd "$ROOT_PATH"
	dotnet build --no-incremental

	if [ $? -eq 0 ]; then
		echo -e "\n${GREEN}[SUCCESS] Build completed successfully!${NC}"
	else
		echo -e "\n${RED}[ERROR] Build failed!${NC}"
		exit 1
	fi
}

run_tests() {
	show_banner
	echo -e "${BLUE}[INFO] Running all tests...${NC}"

	cd "$ROOT_PATH"

	echo -e "\n${CYAN}--- Unit Tests ---${NC}"
	dotnet test tests/OrderManagement.UnitTests --no-build --verbosity minimal

	echo -e "\n${CYAN}--- Integration Tests ---${NC}"
	dotnet test tests/OrderManagement.IntegrationTests --no-build --verbosity minimal

	if [ $? -eq 0 ]; then
		echo -e "\n${GREEN}[SUCCESS] All tests passed!${NC}"
	else
		echo -e "\n${RED}[ERROR] Some tests failed!${NC}"
		exit 1
	fi
}

run_clean() {
	show_banner
	echo -e "${BLUE}[INFO] Cleaning build artifacts...${NC}"

	cd "$ROOT_PATH"

	dotnet clean

	echo -e "${GRAY}[INFO] Removing bin and obj directories...${NC}"
	find "$ROOT_PATH" -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true

	echo -e "${GREEN}[SUCCESS] Clean completed!${NC}"
}

# Main execution
case "$COMMAND" in
	run)
		run_api
		;;
	watch)
		run_watch
		;;
	build)
		run_build
		;;
	test)
		run_tests
		;;
	clean)
		run_clean
		;;
	help|--help|-h)
		show_help
		;;
	*)
		echo -e "${RED}[ERROR] Unknown command: $COMMAND${NC}\n"
		show_help
		exit 1
		;;
esac
