-- Mangalith Database Initialization Script
-- This script sets up the initial database configuration

-- Create extensions if they don't exist
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create indexes for text search optimization
-- These will be used by Entity Framework migrations but we prepare the extensions

-- Set timezone to UTC for consistency
SET timezone = 'UTC';

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'Mangalith database initialized successfully at %', NOW();
END $$;