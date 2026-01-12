@echo off
REM ============================================================================
REM Local PostgreSQL Database Setup Script (Windows)
REM ============================================================================
REM This script runs the schema and seed SQL files against your local database.
REM
REM Prerequisites:
REM - PostgreSQL installed and running
REM - Database 'jeal_prototype' created
REM - Update the password in the commands below
REM ============================================================================

echo.
echo ============================================================================
echo Setting up Local PostgreSQL Database
echo ============================================================================
echo.

REM Replace 'your_password' with your actual PostgreSQL password
set PGPASSWORD=your_password
set DB_HOST=localhost
set DB_PORT=5432
set DB_NAME=jeal_prototype
set DB_USER=postgres

echo Step 1: Running schema.sql (creating tables)...
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -f schema.sql
if errorlevel 1 (
    echo ERROR: Schema creation failed!
    pause
    exit /b 1
)
echo ✓ Schema created successfully
echo.

echo Step 2: Running seed.sql (inserting sample data)...
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -f seed.sql
if errorlevel 1 (
    echo ERROR: Seed data insertion failed!
    pause
    exit /b 1
)
echo ✓ Seed data inserted successfully
echo.

echo Step 3: Verifying database setup...
echo.
echo Dealership count (expected: 2):
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -c "SELECT COUNT(*) FROM dealership;"
echo.
echo Vehicle count (expected: 10):
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -c "SELECT COUNT(*) FROM vehicle;"
echo.
echo Lead count (expected: 5):
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -c "SELECT COUNT(*) FROM lead;"
echo.

echo ============================================================================
echo Database setup complete!
echo ============================================================================
echo.
echo Sample dealerships:
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -c "SELECT id, name FROM dealership;"
echo.
pause
