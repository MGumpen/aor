#!/bin/bash

# Build and run the application using Docker Compose
echo "Building and starting AOR application..."

# Stop any running containers
docker-compose down

# Build and start the containers
docker-compose up --build -d

echo "Application is running at:"
echo "HTTP: http://localhost:5000"

echo ""
echo "To view logs: docker-compose logs -f"
echo "To stop: docker-compose down"
