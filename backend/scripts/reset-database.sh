#!/bin/bash

# Reset Database Script for Mangalith
# This script drops and recreates the database, then runs migrations
# WARNING: This script is for DEVELOPMENT use only!

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
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

# Function to show usage
show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -f, --force             Skip confirmation prompt"
    echo "  -b, --backup            Create backup before reset"
    echo "  -k, --keep-migrations   Keep existing migration files"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                      # Interactive reset"
    echo "  $0 --force --backup     # Force reset with backup"
    echo "  $0 --keep-migrations    # Reset but keep migration files"
}

# Default values
FORCE=false
BACKUP=false
KEEP_MIGRATIONS=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -f|--force)
            FORCE=true
            shift
            ;;
        -b|--backup)
            BACKUP=true
            shift
            ;;
        -k|--keep-migrations)
            KEEP_MIGRATIONS=true
            shift
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

print_warning "🗑️  DANGER: This will completely reset the Mangalith database!"
print_warning "This script is intended for DEVELOPMENT use only."

# Load environment variables from root (Docker Compose config)
if [ -f "../../.env" ]; then
    export $(cat ../../.env | grep -v '^#' | xargs)
    print_status "Loaded environment variables from ../../.env"
else
    print_warning "Environment file ../../.env not found, using defaults"
fi

# Default values
POSTGRES_DB=${POSTGRES_DB:-mangalith}
POSTGRES_USER=${POSTGRES_USER:-mangalith}
POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-mangalith123}
POSTGRES_HOST=${POSTGRES_HOST:-localhost}
POSTGRES_PORT=${POSTGRES_PORT:-5432}

print_status "Database: $POSTGRES_DB"
print_status "User: $POSTGRES_USER"
print_status "Host: $POSTGRES_HOST:$POSTGRES_PORT"

# Safety check for production
if [[ "$POSTGRES_HOST" != "localhost" && "$POSTGRES_HOST" != "127.0.0.1" ]] && [ "$FORCE" = false ]; then
    print_error "Remote database detected: $POSTGRES_HOST"
    print_error "This script should only be used with local development databases."
    print_error "Use --force flag if you're absolutely sure you want to proceed."
    exit 1
fi

# Confirmation prompt
if [ "$FORCE" = false ]; then
    echo ""
    print_warning "This will:"
    echo "  - Drop the entire database '$POSTGRES_DB'"
    echo "  - Recreate the database"
    if [ "$KEEP_MIGRATIONS" = false ]; then
        echo "  - Remove all migration files"
    fi
    echo "  - Apply all migrations"
    echo "  - Seed development data when you run the application"
    echo ""
    read -p "Are you sure you want to continue? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_status "Operation cancelled."
        exit 0
    fi
fi

# Check if PostgreSQL is running
print_status "🔍 Checking database connectivity..."
if ! PGPASSWORD=$POSTGRES_PASSWORD pg_isready -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER > /dev/null 2>&1; then
    print_error "PostgreSQL is not running or not accessible"
    print_error "Make sure to run: docker-compose up -d postgres"
    exit 1
fi
print_success "PostgreSQL is accessible"

# Create backup if requested
if [ "$BACKUP" = true ]; then
    print_status "📦 Creating database backup..."
    BACKUP_FILE="backup_$(date +%Y%m%d_%H%M%S).sql"
    
    # Check if database exists before backup
    DB_EXISTS=$(PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -lqt | cut -d \| -f 1 | grep -qw $POSTGRES_DB && echo "true" || echo "false")
    
    if [ "$DB_EXISTS" = "true" ]; then
        PGPASSWORD=$POSTGRES_PASSWORD pg_dump -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER $POSTGRES_DB > "$BACKUP_FILE"
        print_success "Backup created: $BACKUP_FILE"
    else
        print_warning "Database does not exist, skipping backup"
    fi
fi

# Drop database if exists
print_status "🗑️  Dropping database if exists..."
PGPASSWORD=$POSTGRES_PASSWORD dropdb -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER --if-exists $POSTGRES_DB

# Create database
print_status "🆕 Creating database..."
PGPASSWORD=$POSTGRES_PASSWORD createdb -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER $POSTGRES_DB

# Navigate to API project directory
cd Mangalith.Api

# Remove existing migrations if requested
if [ "$KEEP_MIGRATIONS" = false ]; then
    print_status "🧹 Cleaning existing migrations..."
    if [ -d "../Mangalith.Infrastructure/Migrations" ]; then
        rm -rf ../Mangalith.Infrastructure/Migrations
        print_success "Migration files removed"
    else
        print_status "No migration files found to remove"
    fi
else
    print_status "Keeping existing migration files"
fi

# Apply migrations
print_status "⬆️  Applying migrations..."
dotnet ef database update --context MangalithDbContext --project ../Mangalith.Infrastructure

print_success "✅ Database reset completed successfully!"
print_status "🌱 The database will be seeded automatically when you run the application in Development mode."

# Show next steps
print_status "💡 Next steps:"
echo "  1. Run 'dotnet run' to start the API server"
echo "  2. The application will automatically seed development data"
echo "  3. Use the validation script to verify the setup:"
echo "     ./validate-permission-system.sh"

# Show available test accounts
print_status "🔑 Test accounts that will be created:"
echo "  - admin@mangalith.local (password: admin123) - Administrator"
echo "  - moderator@mangalith.local (password: moderator123) - Moderator"
echo "  - uploader@mangalith.local (password: uploader123) - Uploader"
echo "  - reader@mangalith.local (password: reader123) - Reader"