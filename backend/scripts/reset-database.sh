#!/bin/bash

# Reset Database Script for Mangalith
# This script drops and recreates the database, then runs migrations

set -e

echo "🗑️  Resetting Mangalith database..."

# Load environment variables from root (Docker Compose config)
if [ -f "../../.env" ]; then
    export $(cat ../../.env | grep -v '^#' | xargs)
fi

# Default values
POSTGRES_DB=${POSTGRES_DB:-mangalith}
POSTGRES_USER=${POSTGRES_USER:-mangalith}
POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-mangalith123}
POSTGRES_HOST=${POSTGRES_HOST:-localhost}
POSTGRES_PORT=${POSTGRES_PORT:-5432}

echo "📊 Database: $POSTGRES_DB"
echo "👤 User: $POSTGRES_USER"
echo "🏠 Host: $POSTGRES_HOST:$POSTGRES_PORT"

# Check if PostgreSQL is running
if ! pg_isready -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER > /dev/null 2>&1; then
    echo "❌ PostgreSQL is not running or not accessible"
    echo "💡 Make sure to run: docker-compose up -d postgres"
    exit 1
fi

echo "✅ PostgreSQL is running"

# Drop database if exists
echo "🗑️  Dropping database if exists..."
PGPASSWORD=$POSTGRES_PASSWORD dropdb -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER --if-exists $POSTGRES_DB

# Create database
echo "🆕 Creating database..."
PGPASSWORD=$POSTGRES_PASSWORD createdb -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER $POSTGRES_DB

# Navigate to API project directory
cd ../Mangalith.Api

# Remove existing migrations
echo "🧹 Cleaning existing migrations..."
if [ -d "Migrations" ]; then
    rm -rf Migrations
fi

# Create new migration
echo "📝 Creating initial migration..."
dotnet ef migrations add InitialCreate --context MangalithDbContext --project ../Mangalith.Infrastructure

# Apply migration
echo "⬆️  Applying migration..."
dotnet ef database update --context MangalithDbContext --project ../Mangalith.Infrastructure

echo "✅ Database reset completed successfully!"
echo "🌱 The database will be seeded automatically when you run the application in Development mode."