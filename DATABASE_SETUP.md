# Database Setup Guide

This guide provides instructions for setting up the PostgreSQL database for local development.

## Quick Start Options

Choose ONE of the following methods:

### Option A: Docker (Recommended - Easiest)

**Prerequisites:** Docker Desktop installed on Windows

**Steps:**

1. **Start PostgreSQL container:**
   ```bash
   docker-compose up -d
   ```

2. **Update .env file:**
   ```bash
   # Uncomment the Docker DATABASE_URL line in .env:
   DATABASE_URL=postgresql://postgres:postgres@localhost:5432/jeal_prototype
   ```

3. **Run database setup scripts:**
   ```bash
   # Windows Command Prompt or PowerShell
   docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\schema.sql
   docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\seed.sql
   ```

4. **Verify setup:**
   ```bash
   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM dealership;"
   # Expected output: 2

   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM vehicle;"
   # Expected output: 10

   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM lead;"
   # Expected output: 5
   ```

5. **View sample data:**
   ```bash
   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name FROM dealership;"
   ```

**To stop the database:**
```bash
docker-compose down
```

**To stop and remove all data:**
```bash
docker-compose down -v
```

---

### Option B: Local PostgreSQL Installation

**Prerequisites:** PostgreSQL 14+ installed on Windows

**Steps:**

1. **Download and install PostgreSQL:**
   - Download from: https://www.postgresql.org/download/windows/
   - Run installer and remember the password you set for 'postgres' user
   - Default port: 5432

2. **Create database:**
   ```bash
   # Open Command Prompt and run:
   psql -U postgres

   # At the postgres=# prompt:
   CREATE DATABASE jeal_prototype;
   \q
   ```

3. **Update .env file:**
   ```bash
   # Edit .env and replace 'your_password' with your PostgreSQL password:
   DATABASE_URL=postgresql://postgres:your_password@localhost:5432/jeal_prototype
   ```

4. **Run database setup using batch script:**
   ```bash
   # Navigate to backend/db directory
   cd backend\db

   # Edit setup-local.bat and update the password on line 20:
   # set PGPASSWORD=your_password

   # Run the setup script
   setup-local.bat
   ```

**OR manually run SQL scripts:**

```bash
# Set password as environment variable (replace 'your_password')
set PGPASSWORD=your_password

# Run schema script
psql -h localhost -p 5432 -U postgres -d jeal_prototype -f backend\db\schema.sql

# Run seed script
psql -h localhost -p 5432 -U postgres -d jeal_prototype -f backend\db\seed.sql

# Verify setup
psql -h localhost -p 5432 -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM dealership;"
psql -h localhost -p 5432 -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM vehicle;"
psql -h localhost -p 5432 -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM lead;"
```

---

## Database Schema Overview

### Tables Created:

1. **dealership** - Stores dealership profile information
   - 2 sample dealerships: "Acme Auto Sales" and "Premier Motors"

2. **vehicle** - Stores vehicle inventory
   - 10 sample vehicles (5 per dealership)
   - Varied status: active, pending, sold, draft

3. **lead** - Stores customer enquiries
   - 5 sample leads (3 for Acme, 2 for Premier)

### Indexes Created:

- `idx_vehicle_dealership_id` - Vehicle queries by dealership
- `idx_lead_dealership_id` - Lead queries by dealership
- `idx_vehicle_status` - Public site filtering
- `idx_lead_created_at` - Lead inbox sorting
- `idx_vehicle_dealership_status` - Composite index for common queries

---

## Testing Database Connection

After setting up the database, test the connection from your backend server:

```bash
# From project root
cd backend
node -e "require('dotenv').config({path:'../.env'}); const pool = require('./db/index.js');"
```

Expected output:
```
Database connected successfully at [timestamp]
```

---

## Troubleshooting

### Error: "password authentication failed"
- **Docker:** Password is `postgres` (check docker-compose.yml)
- **Local:** Verify password in .env matches your PostgreSQL installation password

### Error: "database does not exist"
- **Docker:** Database is auto-created by docker-compose
- **Local:** Run `psql -U postgres` then `CREATE DATABASE jeal_prototype;`

### Error: "psql: command not found"
- **Docker:** Ensure Docker Desktop is running
- **Local:** Add PostgreSQL bin directory to your PATH environment variable
  - Default location: `C:\Program Files\PostgreSQL\14\bin`

### Error: "relation does not exist"
- The schema.sql script hasn't been run yet
- Re-run: `psql ... < backend/db/schema.sql`

### Error: "port 5432 is already in use"
- Another PostgreSQL instance is running
- Either stop the other instance or change the port in docker-compose.yml and .env

---

## Next Steps

After database setup is complete:

1. Start the backend server: `npm run server`
2. Verify "Database connected successfully" appears in console
3. Test API endpoints (will be created in upcoming stories)

---

## Production Deployment (Railway)

For production deployment to Railway:

1. Create Railway account: https://railway.app
2. Create new project and add PostgreSQL plugin
3. Copy DATABASE_URL from Railway dashboard
4. Update .env with Railway DATABASE_URL
5. Run schema and seed scripts:
   ```bash
   railway run psql $DATABASE_URL < backend/db/schema.sql
   railway run psql $DATABASE_URL < backend/db/seed.sql
   ```
