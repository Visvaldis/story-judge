#!/bin/bash

# StoryJudge - Development Startup Script
# This script starts MongoDB, the .NET backend, and the Vue frontend

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/backend"
FRONTEND_DIR="$SCRIPT_DIR/frontend"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}   StoryJudge Development Server${NC}"
echo -e "${BLUE}========================================${NC}"
echo

# Function to cleanup on exit
cleanup() {
    echo
    echo -e "${YELLOW}Shutting down services...${NC}"

    # Kill background processes
    if [ ! -z "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null || true
    fi
    if [ ! -z "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null || true
    fi

    # Stop MongoDB
    echo -e "${YELLOW}Stopping MongoDB...${NC}"
    cd "$BACKEND_DIR" && docker-compose down 2>/dev/null || true

    echo -e "${GREEN}All services stopped.${NC}"
    exit 0
}

trap cleanup SIGINT SIGTERM

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

if ! command -v docker &> /dev/null; then
    echo -e "${RED}Error: Docker is not installed${NC}"
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed${NC}"
    exit 1
fi

if ! command -v npm &> /dev/null; then
    echo -e "${RED}Error: Node.js/npm is not installed${NC}"
    exit 1
fi

echo -e "${GREEN}All prerequisites met!${NC}"
echo

# Start MongoDB
echo -e "${YELLOW}Starting MongoDB...${NC}"
cd "$BACKEND_DIR"
docker-compose up -d

# Wait for MongoDB to be ready
echo -e "${YELLOW}Waiting for MongoDB to be ready...${NC}"
sleep 3

# Check if MongoDB is running
if docker-compose ps | grep -q "Up"; then
    echo -e "${GREEN}MongoDB is running!${NC}"
else
    echo -e "${RED}Failed to start MongoDB${NC}"
    exit 1
fi
echo

# Install frontend dependencies if needed
if [ ! -d "$FRONTEND_DIR/node_modules" ]; then
    echo -e "${YELLOW}Installing frontend dependencies...${NC}"
    cd "$FRONTEND_DIR"
    npm install
    echo -e "${GREEN}Frontend dependencies installed!${NC}"
    echo
fi

# Start Backend
echo -e "${YELLOW}Starting .NET Backend on http://localhost:5000...${NC}"
cd "$BACKEND_DIR"
dotnet run --project src/StoryJudge.Api --urls "http://localhost:5000" &
BACKEND_PID=$!

# Wait for backend to start
echo -e "${YELLOW}Waiting for backend to start...${NC}"
sleep 5

# Start Frontend
echo -e "${YELLOW}Starting Vue Frontend on http://localhost:5173...${NC}"
cd "$FRONTEND_DIR"
npm run dev &
FRONTEND_PID=$!

sleep 3

echo
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}   All services are running!${NC}"
echo -e "${GREEN}========================================${NC}"
echo
echo -e "  ${BLUE}Frontend:${NC}  http://localhost:5173"
echo -e "  ${BLUE}Backend:${NC}   http://localhost:5000"
echo -e "  ${BLUE}Swagger:${NC}   http://localhost:5000/swagger"
echo -e "  ${BLUE}MongoDB:${NC}   localhost:27017"
echo
echo -e "${YELLOW}Press Ctrl+C to stop all services${NC}"
echo

# Wait for processes
wait
