#!/bin/bash

# Permission System Data Validation Script for Mangalith
# This script validates the integrity of the permission system data

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
    echo "  -f, --fix               Attempt to fix validation issues"
    echo "  -v, --verbose           Verbose output"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 --environment Production --verbose"
    echo "  $0 --fix"
}

# Default values
ENVIRONMENT="Development"
FIX_ISSUES=false
VERBOSE=false
VALIDATION_ERRORS=0

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -f|--fix)
            FIX_ISSUES=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
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

print_status "🔍 Starting permission system validation..."
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

# Check database connectivity
print_status "🔍 Checking database connectivity..."
if ! PGPASSWORD=$POSTGRES_PASSWORD pg_isready -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER > /dev/null 2>&1; then
    print_error "PostgreSQL is not running or not accessible"
    exit 1
fi
print_success "Database is accessible"

# Function to execute SQL query and return result
execute_query() {
    local query="$1"
    PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB -t -c "$query" | xargs
}

# Function to execute SQL query and return all results
execute_query_all() {
    local query="$1"
    PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB -t -c "$query"
}

print_status "📊 Validating permission system data integrity..."

# 1. Check if all required tables exist
print_status "1. Checking required tables..."
REQUIRED_TABLES=("Users" "Permissions" "RolePermissions" "AuditLogs" "UserInvitations" "UserQuotas" "RateLimitEntries")

for table in "${REQUIRED_TABLES[@]}"; do
    TABLE_EXISTS=$(execute_query "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = '${table}');")
    
    if [ "$TABLE_EXISTS" = "t" ]; then
        print_success "✓ Table $table exists"
    else
        print_error "✗ Table $table is missing"
        ((VALIDATION_ERRORS++))
    fi
done

# 2. Check permissions data integrity
print_status "2. Validating permissions data..."

PERMISSION_COUNT=$(execute_query "SELECT COUNT(*) FROM \"Permissions\";")
print_status "Found $PERMISSION_COUNT permissions"

if [ "$PERMISSION_COUNT" -eq 0 ]; then
    print_error "✗ No permissions found in database"
    ((VALIDATION_ERRORS++))
else
    # Check for duplicate permissions
    DUPLICATE_PERMISSIONS=$(execute_query "SELECT COUNT(*) FROM (SELECT \"Resource\", \"Action\", COUNT(*) FROM \"Permissions\" GROUP BY \"Resource\", \"Action\" HAVING COUNT(*) > 1) AS duplicates;")
    
    if [ "$DUPLICATE_PERMISSIONS" -eq 0 ]; then
        print_success "✓ No duplicate permissions found"
    else
        print_error "✗ Found $DUPLICATE_PERMISSIONS duplicate permissions"
        ((VALIDATION_ERRORS++))
        
        if [ "$VERBOSE" = true ]; then
            print_status "Duplicate permissions:"
            execute_query_all "SELECT \"Resource\", \"Action\", COUNT(*) FROM \"Permissions\" GROUP BY \"Resource\", \"Action\" HAVING COUNT(*) > 1;"
        fi
    fi
    
    # Check for permissions with invalid format
    INVALID_PERMISSIONS=$(execute_query "SELECT COUNT(*) FROM \"Permissions\" WHERE \"Resource\" = '' OR \"Action\" = '' OR \"Description\" = '';")
    
    if [ "$INVALID_PERMISSIONS" -eq 0 ]; then
        print_success "✓ All permissions have valid format"
    else
        print_error "✗ Found $INVALID_PERMISSIONS permissions with invalid format"
        ((VALIDATION_ERRORS++))
    fi
fi

# 3. Check role permissions mapping
print_status "3. Validating role permissions mapping..."

ROLE_PERMISSION_COUNT=$(execute_query "SELECT COUNT(*) FROM \"RolePermissions\";")
print_status "Found $ROLE_PERMISSION_COUNT role-permission mappings"

if [ "$ROLE_PERMISSION_COUNT" -eq 0 ]; then
    print_error "✗ No role permissions found in database"
    ((VALIDATION_ERRORS++))
else
    # Check for orphaned role permissions (permissions that don't exist)
    ORPHANED_ROLE_PERMISSIONS=$(execute_query "SELECT COUNT(*) FROM \"RolePermissions\" rp LEFT JOIN \"Permissions\" p ON rp.\"PermissionId\" = p.\"Id\" WHERE p.\"Id\" IS NULL;")
    
    if [ "$ORPHANED_ROLE_PERMISSIONS" -eq 0 ]; then
        print_success "✓ No orphaned role permissions found"
    else
        print_error "✗ Found $ORPHANED_ROLE_PERMISSIONS orphaned role permissions"
        ((VALIDATION_ERRORS++))
    fi
    
    # Check if all roles have at least one permission
    for role in 0 1 2 3; do  # Reader=0, Uploader=1, Moderator=2, Administrator=3
        ROLE_NAME=""
        case $role in
            0) ROLE_NAME="Reader" ;;
            1) ROLE_NAME="Uploader" ;;
            2) ROLE_NAME="Moderator" ;;
            3) ROLE_NAME="Administrator" ;;
        esac
        
        ROLE_PERMISSIONS=$(execute_query "SELECT COUNT(*) FROM \"RolePermissions\" WHERE \"Role\" = $role;")
        
        if [ "$ROLE_PERMISSIONS" -gt 0 ]; then
            print_success "✓ $ROLE_NAME role has $ROLE_PERMISSIONS permissions"
        else
            print_error "✗ $ROLE_NAME role has no permissions assigned"
            ((VALIDATION_ERRORS++))
        fi
    done
fi

# 4. Check user data integrity
print_status "4. Validating user data..."

USER_COUNT=$(execute_query "SELECT COUNT(*) FROM \"Users\";")
print_status "Found $USER_COUNT users"

if [ "$USER_COUNT" -gt 0 ]; then
    # Check for users with invalid roles
    INVALID_ROLE_USERS=$(execute_query "SELECT COUNT(*) FROM \"Users\" WHERE \"Role\" NOT IN (0, 1, 2, 3);")
    
    if [ "$INVALID_ROLE_USERS" -eq 0 ]; then
        print_success "✓ All users have valid roles"
    else
        print_error "✗ Found $INVALID_ROLE_USERS users with invalid roles"
        ((VALIDATION_ERRORS++))
    fi
    
    # Check for users without email
    USERS_WITHOUT_EMAIL=$(execute_query "SELECT COUNT(*) FROM \"Users\" WHERE \"Email\" = '' OR \"Email\" IS NULL;")
    
    if [ "$USERS_WITHOUT_EMAIL" -eq 0 ]; then
        print_success "✓ All users have email addresses"
    else
        print_error "✗ Found $USERS_WITHOUT_EMAIL users without email addresses"
        ((VALIDATION_ERRORS++))
    fi
fi

# 5. Check audit log integrity
print_status "5. Validating audit logs..."

AUDIT_LOG_COUNT=$(execute_query "SELECT COUNT(*) FROM \"AuditLogs\";")
print_status "Found $AUDIT_LOG_COUNT audit log entries"

if [ "$AUDIT_LOG_COUNT" -gt 0 ]; then
    # Check for audit logs with missing user references
    ORPHANED_AUDIT_LOGS=$(execute_query "SELECT COUNT(*) FROM \"AuditLogs\" al LEFT JOIN \"Users\" u ON al.\"UserId\" = u.\"Id\" WHERE u.\"Id\" IS NULL;")
    
    if [ "$ORPHANED_AUDIT_LOGS" -eq 0 ]; then
        print_success "✓ All audit logs have valid user references"
    else
        print_error "✗ Found $ORPHANED_AUDIT_LOGS audit logs with invalid user references"
        ((VALIDATION_ERRORS++))
    fi
    
    # Check for audit logs with empty required fields
    INVALID_AUDIT_LOGS=$(execute_query "SELECT COUNT(*) FROM \"AuditLogs\" WHERE \"Action\" = '' OR \"Resource\" = '' OR \"IpAddress\" = '';")
    
    if [ "$INVALID_AUDIT_LOGS" -eq 0 ]; then
        print_success "✓ All audit logs have required fields"
    else
        print_error "✗ Found $INVALID_AUDIT_LOGS audit logs with missing required fields"
        ((VALIDATION_ERRORS++))
    fi
fi

# 6. Check user invitations integrity
print_status "6. Validating user invitations..."

INVITATION_COUNT=$(execute_query "SELECT COUNT(*) FROM \"UserInvitations\";")
print_status "Found $INVITATION_COUNT user invitations"

if [ "$INVITATION_COUNT" -gt 0 ]; then
    # Check for expired invitations that should be cleaned up
    EXPIRED_INVITATIONS=$(execute_query "SELECT COUNT(*) FROM \"UserInvitations\" WHERE \"ExpiresAtUtc\" < NOW() AND \"AcceptedAtUtc\" IS NULL;")
    
    if [ "$EXPIRED_INVITATIONS" -eq 0 ]; then
        print_success "✓ No expired invitations found"
    else
        print_warning "⚠ Found $EXPIRED_INVITATIONS expired invitations that could be cleaned up"
        
        if [ "$FIX_ISSUES" = true ]; then
            print_status "Cleaning up expired invitations..."
            execute_query "DELETE FROM \"UserInvitations\" WHERE \"ExpiresAtUtc\" < NOW() AND \"AcceptedAtUtc\" IS NULL;"
            print_success "Cleaned up expired invitations"
        fi
    fi
    
    # Check for invitations with invalid roles
    INVALID_ROLE_INVITATIONS=$(execute_query "SELECT COUNT(*) FROM \"UserInvitations\" WHERE \"TargetRole\" NOT IN (0, 1, 2, 3);")
    
    if [ "$INVALID_ROLE_INVITATIONS" -eq 0 ]; then
        print_success "✓ All invitations have valid target roles"
    else
        print_error "✗ Found $INVALID_ROLE_INVITATIONS invitations with invalid target roles"
        ((VALIDATION_ERRORS++))
    fi
fi

# 7. Check user quotas integrity
print_status "7. Validating user quotas..."

QUOTA_COUNT=$(execute_query "SELECT COUNT(*) FROM \"UserQuotas\";")
print_status "Found $QUOTA_COUNT user quota records"

if [ "$QUOTA_COUNT" -gt 0 ]; then
    # Check for negative values in quotas
    INVALID_QUOTAS=$(execute_query "SELECT COUNT(*) FROM \"UserQuotas\" WHERE \"StorageUsedBytes\" < 0 OR \"FilesUploadedToday\" < 0 OR \"MangasCreated\" < 0;")
    
    if [ "$INVALID_QUOTAS" -eq 0 ]; then
        print_success "✓ All quota values are valid"
    else
        print_error "✗ Found $INVALID_QUOTAS quota records with invalid values"
        ((VALIDATION_ERRORS++))
    fi
    
    # Check for orphaned quota records
    ORPHANED_QUOTAS=$(execute_query "SELECT COUNT(*) FROM \"UserQuotas\" uq LEFT JOIN \"Users\" u ON uq.\"UserId\" = u.\"Id\" WHERE u.\"Id\" IS NULL;")
    
    if [ "$ORPHANED_QUOTAS" -eq 0 ]; then
        print_success "✓ All quota records have valid user references"
    else
        print_error "✗ Found $ORPHANED_QUOTAS orphaned quota records"
        ((VALIDATION_ERRORS++))
    fi
fi

# 8. Check database constraints and indexes
print_status "8. Validating database constraints and indexes..."

# Check foreign key constraints
CONSTRAINT_VIOLATIONS=$(execute_query "SELECT COUNT(*) FROM pg_constraint WHERE NOT convalidated;")

if [ "$CONSTRAINT_VIOLATIONS" -eq 0 ]; then
    print_success "✓ All database constraints are valid"
else
    print_error "✗ Found $CONSTRAINT_VIOLATIONS invalid constraints"
    ((VALIDATION_ERRORS++))
fi

# Check critical indexes exist
CRITICAL_INDEXES=("IX_Permissions_Resource_Action" "IX_RolePermissions_Role" "IX_AuditLogs_UserId" "IX_UserInvitations_Token")

for index in "${CRITICAL_INDEXES[@]}"; do
    INDEX_EXISTS=$(execute_query "SELECT EXISTS (SELECT FROM pg_indexes WHERE indexname = '${index}');")
    
    if [ "$INDEX_EXISTS" = "t" ]; then
        print_success "✓ Index $index exists"
    else
        print_error "✗ Critical index $index is missing"
        ((VALIDATION_ERRORS++))
    fi
done

# Summary
print_status "📊 Validation Summary"
if [ "$VALIDATION_ERRORS" -eq 0 ]; then
    print_success "🎉 Permission system validation completed successfully!"
    print_success "All data integrity checks passed."
else
    print_error "❌ Validation completed with $VALIDATION_ERRORS errors"
    print_error "Please review and fix the issues above."
    
    if [ "$FIX_ISSUES" = false ]; then
        print_status "💡 Run with --fix flag to attempt automatic fixes for some issues"
    fi
    
    exit 1
fi

# Additional recommendations
print_status "💡 Recommendations:"
if [ "$ENVIRONMENT" = "Production" ]; then
    echo "  - Schedule regular validation checks"
    echo "  - Monitor audit log growth and implement archival"
    echo "  - Review user quota usage patterns"
    echo "  - Set up alerts for constraint violations"
else
    echo "  - Run validation after major data changes"
    echo "  - Use this script in CI/CD pipeline"
    echo "  - Test permission changes in development first"
fi