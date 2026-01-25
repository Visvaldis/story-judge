#!/bin/bash

# StoryJudge - Stop Script

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/backend"

echo "Stopping StoryJudge services..."

# Kill any running dotnet processes for this project
pkill -f "StoryJudge.Api" 2>/dev/null || true

# Kill any running vite dev server
pkill -f "vite" 2>/dev/null || true

# Stop MongoDB
cd "$BACKEND_DIR"
docker-compose down 2>/dev/null || true

echo "All services stopped."
