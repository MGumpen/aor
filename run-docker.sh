#!/bin/bash

echo "🚀 Setting up AOR application..."

# Stop any conflicting containers that might use the same ports
echo "🛑 Stopping any conflicting containers..."
docker stop aor-web aor-mariadb aor-adminer mariadbcontainer mysql-fa47c6ea kartverket-app aor 2>/dev/null || true
docker rm aor-web aor-mariadb aor-adminer mariadbcontainer mysql-fa47c6ea kartverket-app aor 2>/dev/null || true

# Stop any running containers from this compose
echo "🔄 Cleaning up previous AOR containers..."
docker-compose down -v 2>/dev/null || true

# Build and start the containers
echo "🏗️  Building and starting AOR application..."
docker-compose up --build -d

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 10

# Check if containers are running
if docker ps | grep -q "aor-mariadb"; then
    echo "✅ MariaDB is running"
else
    echo "❌ MariaDB failed to start"
    exit 1
fi

if docker ps | grep -q "aor-web"; then
    echo "✅ AOR Web application is running"
else
    echo "❌ AOR Web application failed to start"
    exit 1
fi

# Wait a bit more for the web app to be ready
echo "⏳ Waiting for web application to be ready..."
sleep 5

# Test if the application responds
for i in {1..30}; do
    if curl -s http://localhost > /dev/null 2>&1; then
        echo "✅ Application is responding!"
        break
    fi
    echo "⏳ Still waiting for application... ($i/30)"
    sleep 2
done

echo ""
echo "🎉 AOR Application is running!"
echo "📱 Opening http://localhost in your browser..."
echo ""
echo "Available services:"
echo "  • Main app: http://localhost"
echo "  • Database admin: http://localhost:8081 (Adminer)"
echo "  • Health check: http://localhost/health"
echo "  • DB health: http://localhost/db-health"
echo ""
echo "📋 Useful commands:"
echo "  • View logs: docker-compose logs -f"
echo "  • Stop: docker-compose down"
echo "  • Restart: ./run-docker.sh"

# Open browser automatically (works on macOS, Linux, and Windows)
if command -v open > /dev/null 2>&1; then
    # macOS
    open http://localhost
elif command -v xdg-open > /dev/null 2>&1; then
    # Linux
    xdg-open http://localhost
elif command -v start > /dev/null 2>&1; then
    # Windows
    start http://localhost
else
    echo "Please open http://localhost in your browser"
fi
