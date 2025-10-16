#!/bin/bash

# Production Database Migration Script for Mangalith
# This script handles database migrations with rollback support and validation

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
    echo "  -e, --environment ENV    Environment (Development, Staging, Production)"
    echo "  -t, --target MIGRATION   Target migration (optional, defaults to latest)"
    echo "  -r, --rollback          Rollback to previous migration"
    echo "  -v, --validate          Validate database integrity after migration"
    echo "  -b, --backup            Create backup before migration"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 --environment Production --backup --validate"
    echo "  $0 --rollback --target 20251015053041_AddRolePermissionSystem"
    echo "  $0 --validate"
}

# Default values
ENVIRONMENT="Development"
TARGET_MIGRATION=""
ROLLBACK=false
VALIDATE=false
BACKUP=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -t|--target)
            TARGET_MIGRATION="$2"
            shift 2
            ;;
        -r|--rollback)
            ROLLBACK=true
            shift
            ;;
        -v|--validate)
            VALIDATE=true
            shift
            ;;
        -b|--backup)
            BACKUP=true
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

print_status "🚀 Starting Mangalith database migration..."
print_status "Environment: $ENVIRONMENT"

# Load environment variables
ENV_FILE=""
case $ENVIRONMENT in
    "Development")
        ENV_FILE="../../.env"
        ;;
    "Staging")
        ENV_FILE="../../.env.staging"
        ;;
    "Production")
        ENV_FILE="../../.env.production"
        ;;
    *)
        print_error "Invalid environment: $ENVIRONMENT"
        exit 1
        ;;
esac

if [ -f "$ENV_FILE" ]; then
    export $(cat "$ENV_FILE" | grep -v '^#' | xargs)
    print_status "Loaded environment variables from $ENV_FILE"
else
    print_warning "Environment file $ENV_FILE not found, using defaults"
fi

# Database connection parameters
POSTGRES_DB=${POSTGRES_DB:-mangalith}
POSTGRES_USER=${POSTGRES_USER:-mangalith}
POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-mangalith123}
POSTGRES_HOST=${POSTGRES_HOST:-localhost}
POSTGRES_PORT=${POSTGRES_PORT:-5432}

print_status "Database: $POSTGRES_DB"
print_status "Host: $POSTGRES_HOST:$POSTGRES_PORT"
print_status "User: $POSTGRES_USER"

# Check if PostgreSQL is accessible
print_status "🔍 Checking database connectivity..."
if ! PGPASSWORD=$POSTGRES_PASSWORD pg_isready -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER > /dev/null 2>&1; then
    print_error "PostgreSQL is not running or not accessible"
    print_error "Connection string: Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER"
    exit 1
fi
print_success "Database is accessible"

# Create backup if requested
if [ "$BACKUP" = true ]; then
    print_status "📦 Creating database backup..."
    BACKUP_FILE="backup_$(date +%Y%m%d_%H%M%S).sql"
    PGPASSWORD=$POSTGRES_PASSWORD pg_dump -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER $POSTGRES_DB > "$BACKUP_FILE"
    print_success "Backup created: $BACKUP_FILE"
fi

# Navigate to API project directory
cd Mangalith.Api

# Check current migration status
print_status "📊 Checking current migration status..."
CURRENT_MIGRATIONS=$(dotnet ef migrations list --context MangalithDbContext --project ../Mangalith.Infrastructure --no-build 2>/dev/null || echo "")

if [ -z "$CURRENT_MIGRATIONS" ]; then
    print_warning "No migrations found or unable to retrieve migration list"
else
    print_status "Current migrations:"
    echo "$CURRENT_MIGRATIONS"
fi

# Perform migration or rollback
if [ "$ROLLBACK" = true ]; then
    if [ -z "$TARGET_MIGRATION" ]; then
        print_error "Target migration must be specified for rollback"
        exit 1
    fi
    
    print_status "⏪ Rolling back to migration: $TARGET_MIGRATION"
    dotnet ef database update "$TARGET_MIGRATION" --context MangalithDbContext --project ../Mangalith.Infrastructure
    print_success "Rollback completed"
else
    print_status "⬆️ Applying migrations..."
    if [ -n "$TARGET_MIGRATION" ]; then
        print_status "Target migration: $TARGET_MIGRATION"
        dotnet ef database update "$TARGET_MIGRATION" --context MangalithDbContext --project ../Mangalith.Infrastructure
    else
        dotnet ef database update --context MangalithDbContext --project ../Mangalith.Infrastructure
    fi
    print_success "Migrations applied successfully"
fi

# Validate database integrity if requested
if [ "$VALIDATE" = true ]; then
    print_status "🔍 Validating database integrity..."
    
    # Check if all required tables exist
    REQUIRED_TABLES=("Users" "Permissions" "RolePermissions" "AuditLogs" "UserInvitations" "UserQuotas" "RateLimitEntries" "Mangas" "Chapters" "ChapterPages" "MangaFiles")
    
    for table in "${REQUIRED_TABLES[@]}"; do
        TABLE_EXISTS=$(PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB -t -c "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = '${table}');" | xargs)
        
        if [ "$TABLE_EXISTS" = "t" ]; then
            print_success "✓ Table $table exists"
        else
            print_error "✗ Table $table is missing"
            exit 1
        fi
    done
    
    # Check if permissions are properly seeded
    PERMISSION_COUNT=$(PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB -t -c "SELECT COUNT(*) FROM \"Permissions\";" | xargs)
    
    if [ "$PERMISSION_COUNT" -gt 0 ]; then
        print_success "✓ Permissions table has $PERMISSION_COUNT entries"
    else
        print_warning "⚠ Permissions table is empty - run application to seed data"
    fi
    
    # Check if role permissions are properly configured
    ROLE_PERMISSION_COUNT=$(PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB -t -c "SELECT COUNT(*) FROM \"RolePermissions\";" | xargs)
    
    if [ "$ROLE_PERMISSION_COUNT" -gt 0 ]; then
        print_success "✓ Role permissions table has $ROLE_PERMISSION_COUNT entries"
    else
        print_warning "⚠ Role permissions table is empty - run application to seed data"
    fi
    
    # Check database constraints and indexes
    print_status "🔍 Checking database constraints..."
    CONSTRAINT_VIOLATIONS=$(PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB -t -c "
        SELECT COUNT(*) FROM (
            SELECT conname FROM pg_constraint WHERE NOT convalidated
        ) AS invalid_constraints;" | xargs)
    
    if [ "$CONSTRAINT_VIOLATIONS" -eq 0 ]; then
        print_success "✓ All database constraints are valid"
    else
        print_error "✗ Found $CONSTRAINT_VIOLATIONS invalid constraints"
        exit 1
    fi
    
    print_success "Database validation completed successfully"
fi

print_success "🎉 Database migration completed successfully!"

# Show final status
print_status "📊 Final migration status:"
FINAL_MIGRATIONS=$(dotnet ef migrations list --context MangalithDbContext --project ../Mangalith.Infrastructure --no-build 2>/dev/null || echo "Unable to retrieve migration list")
echo "$FINAL_MIGRATIONS"

print_status "💡 Next steps:"
if [ "$ENVIRONMENT" = "Development" ]; then
    echo "  - Run the application to automatically seed development data"
    echo "  - Use 'dotnet run' to start the API server"
else
    echo "  - Ensure application configuration is correct for $ENVIRONMENT"
    echo "  - Monitor application logs for any issues"
    echo "  - Run data validation scripts if available"
fi