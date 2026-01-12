# 8. Database Schema

Complete PostgreSQL schema with copy-pasteable SQL scripts. Run once on Railway to set up database.

## Complete Schema Script

**File:** `backend/db/schema.sql`

```sql
-- ============================================
-- JealPrototypeTest Database Schema
-- Multi-Dealership Car Website + CMS Platform
-- ============================================

-- Drop existing tables (for clean setup)
DROP TABLE IF EXISTS lead CASCADE;
DROP TABLE IF EXISTS vehicle CASCADE;
DROP TABLE IF EXISTS dealership CASCADE;

-- ============================================
-- Dealership Table
-- ============================================
CREATE TABLE dealership (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  logo_url TEXT,
  hero_background_image TEXT,
  theme_color VARCHAR(7) DEFAULT '#3B82F6',
  font_family VARCHAR(100) DEFAULT 'system',
  navigation_config JSONB DEFAULT NULL,
  address TEXT NOT NULL,
  phone VARCHAR(20) NOT NULL,
  email VARCHAR(255) NOT NULL,
  hours TEXT,
  about TEXT,
  finance_policy TEXT,
  warranty_policy TEXT,
  facebook_url TEXT,
  instagram_url TEXT,
  created_at TIMESTAMP DEFAULT NOW()
);

-- Comments for customization columns
COMMENT ON COLUMN dealership.theme_color IS 'Hex color code for dealership theme (e.g., #3B82F6). Used for header background, buttons, links, and all branding elements. Supports dynamic theming via CSS custom properties.';

COMMENT ON COLUMN dealership.font_family IS 'Font family identifier for dealership website typography (e.g., system, arial, times, georgia). Applied to all text elements site-wide. Options: system, arial, times, georgia, verdana, courier, comic-sans, trebuchet, impact, palatino.';

COMMENT ON COLUMN dealership.navigation_config IS 'JSONB array storing navigation menu configuration. Each item has: id (string), label (string), route (string), icon (string), order (integer), enabled (boolean). Null means use default navigation.';

COMMENT ON COLUMN dealership.facebook_url IS 'Facebook page URL for dealership (displayed in footer). Optional.';

COMMENT ON COLUMN dealership.instagram_url IS 'Instagram profile URL for dealership (displayed in footer). Optional.';

-- ============================================
-- Vehicle Table
-- ============================================
CREATE TABLE vehicle (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  make VARCHAR(100) NOT NULL,
  model VARCHAR(100) NOT NULL,
  year INTEGER NOT NULL,
  price DECIMAL(10,2) NOT NULL,
  mileage INTEGER NOT NULL,
  condition VARCHAR(10) NOT NULL CHECK (condition IN ('new', 'used')),
  status VARCHAR(10) NOT NULL DEFAULT 'draft' CHECK (status IN ('active', 'sold', 'pending', 'draft')),
  title VARCHAR(255) NOT NULL,
  description TEXT,
  images JSONB DEFAULT '[]'::jsonb,
  created_at TIMESTAMP DEFAULT NOW()
);

-- ============================================
-- Lead Table
-- ============================================
CREATE TABLE lead (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  vehicle_id INTEGER REFERENCES vehicle(id) ON DELETE SET NULL,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  message TEXT NOT NULL,
  status VARCHAR(20) DEFAULT 'received' CHECK (status IN ('received', 'in progress', 'done')),
  created_at TIMESTAMP DEFAULT NOW()
);

-- Status field added in v1.2 (2025-12-10) for lead progress tracking
-- Possible values:
--   'received'    - Default status for new leads (not yet contacted)
--   'in progress' - Admin is actively working with the customer
--   'done'        - Lead has been resolved/completed

-- ============================================
-- Indexes for Performance
-- ============================================

-- Multi-tenancy queries (most critical)
CREATE INDEX idx_vehicle_dealership_id ON vehicle(dealership_id);
CREATE INDEX idx_lead_dealership_id ON lead(dealership_id);

-- Status filtering (public site shows only active/pending)
CREATE INDEX idx_vehicle_status ON vehicle(status);

-- Lead sorting (admin inbox sorts by created_at DESC)
CREATE INDEX idx_lead_created_at ON lead(created_at DESC);

-- Composite index for common query: vehicles by dealership + status
CREATE INDEX idx_vehicle_dealership_status ON vehicle(dealership_id, status);
```

## Seed Data Script

**File:** `backend/db/seed.sql`

```sql
-- ============================================
-- Seed Data for 2 Demo Dealerships
-- ============================================

-- Insert 2 Dealerships
INSERT INTO dealership (name, address, phone, email, hours, about, finance_policy, warranty_policy, hero_background_image, theme_color) VALUES
  (
    'Acme Auto Sales',
    '123 Main St, Springfield, IL 62701',
    '(555) 123-4567',
    'sales@acmeauto.com',
    'Mon-Fri: 9:00 AM - 6:00 PM
Sat: 10:00 AM - 4:00 PM
Sun: Closed',
    'Family-owned dealership serving Springfield for over 20 years. We specialize in quality used cars at affordable prices with transparent pricing and no-pressure sales. Come visit us today!',
    'We offer flexible financing options to fit your budget. Our finance team works with multiple lenders to secure competitive rates. Approval typically takes 24-48 hours. We accept all credit types and offer special first-time buyer programs. Contact us for a personalized financing quote.',
    'All vehicles come with a 30-day limited warranty covering major mechanical components. Extended warranty options available for purchase. Certified pre-owned vehicles include comprehensive 12-month/12,000-mile warranty. All warranties include roadside assistance. See dealer for complete warranty terms and conditions.',
    NULL,
    '#3B82F6'
  ),
  (
    'Premier Motors',
    '456 Oak Ave, Springfield, IL 62702',
    '(555) 987-6543',
    'info@premiermotors.com',
    'Mon-Sat: 9:00 AM - 7:00 PM
Sun: 11:00 AM - 5:00 PM',
    'Premier Motors offers a wide selection of certified pre-owned vehicles with extended warranties. Our experienced sales team is committed to helping you find the perfect vehicle for your needs and budget.',
    'Premier financing solutions tailored to your needs. We partner with credit unions and banks to offer rates as low as 2.9% APR for qualified buyers. Lease options available on select vehicles. No down payment required on approved credit. Pre-qualification available online in minutes.',
    'Industry-leading warranty coverage on all certified vehicles. Comprehensive bumper-to-bumper coverage for 24 months/24,000 miles. Powertrain warranty extends to 5 years/60,000 miles. All warranties fully transferable. Warranty service available at any authorized service center nationwide. Zero deductible on covered repairs.',
    NULL,
    '#059669'
  );

-- Insert Vehicles for Dealership 1 (Acme Auto Sales)
INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
  (
    1,
    'Toyota',
    'Camry',
    2015,
    15999.99,
    75000,
    'used',
    'active',
    '2015 Toyota Camry SE',
    'Well-maintained sedan with excellent fuel economy. Features include backup camera, Bluetooth connectivity, and premium sound system. Clean title, one owner. Service records available.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Honda',
    'Civic',
    2018,
    18500.00,
    45000,
    'used',
    'active',
    '2018 Honda Civic LX',
    'Low mileage, excellent condition. Automatic transmission, lane departure warning, adaptive cruise control. Non-smoker vehicle, garage kept. Still under manufacturer warranty.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Ford',
    'F-150',
    2017,
    28999.00,
    68000,
    'used',
    'active',
    '2017 Ford F-150 XLT SuperCrew',
    'Powerful and reliable pickup truck. 4WD, towing package, bed liner. Perfect for work or weekend adventures. Recently serviced with new tires.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Chevrolet',
    'Malibu',
    2016,
    13500.00,
    82000,
    'used',
    'pending',
    '2016 Chevrolet Malibu LT',
    'Spacious sedan with modern features. Touchscreen display, remote start, heated seats. Great commuter car with excellent safety ratings.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Nissan',
    'Altima',
    2014,
    11999.00,
    95000,
    'used',
    'sold',
    '2014 Nissan Altima 2.5 S',
    'Affordable and reliable sedan. Bluetooth, backup camera, fuel-efficient 4-cylinder engine. Great first car or daily driver.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  );

-- Insert Vehicles for Dealership 2 (Premier Motors)
INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
  (
    2,
    'BMW',
    '3 Series',
    2019,
    32500.00,
    28000,
    'used',
    'active',
    '2019 BMW 3 Series 330i',
    'Luxury sedan with premium features. Leather seats, navigation, sunroof, premium audio. Certified pre-owned with extended warranty. Meticulously maintained.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Mercedes-Benz',
    'C-Class',
    2018,
    29999.00,
    35000,
    'used',
    'active',
    '2018 Mercedes-Benz C-Class C300',
    'Sophisticated and stylish. Turbocharged engine, advanced safety features, smartphone integration. Service history available. Like-new condition.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Audi',
    'A4',
    2020,
    35999.00,
    22000,
    'used',
    'active',
    '2020 Audi A4 Premium',
    'Low mileage luxury sedan. All-wheel drive, virtual cockpit, premium sound system. Remaining factory warranty. Pristine condition inside and out.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Lexus',
    'RX 350',
    2017,
    31500.00,
    48000,
    'used',
    'active',
    '2017 Lexus RX 350 Luxury',
    'Premium SUV with exceptional reliability. Heated and ventilated seats, panoramic sunroof, navigation. Perfect for families. No accidents.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Acura',
    'TLX',
    2019,
    27500.00,
    32000,
    'used',
    'pending',
    '2019 Acura TLX Technology',
    'Sport luxury sedan with advanced tech. Adaptive suspension, collision mitigation, lane keeping assist. Certified pre-owned with comprehensive warranty.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  );

-- Insert Sample Leads for Dealership 1
INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message) VALUES
  (
    1,
    1,
    'John Doe',
    'john.doe@example.com',
    '(555) 111-2222',
    'I''m interested in the 2015 Toyota Camry. Is it still available? Can I schedule a test drive this weekend?'
  ),
  (
    1,
    2,
    'Jane Smith',
    'jane.smith@example.com',
    '(555) 333-4444',
    'The 2018 Honda Civic looks great! Do you offer financing options? What''s the APR?'
  ),
  (
    1,
    NULL,
    'Bob Johnson',
    'bob.j@example.com',
    '(555) 555-6666',
    'I''m looking for a reliable truck under $30k. Do you have any recommendations based on your current inventory?'
  );

-- Insert Sample Leads for Dealership 2
INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message) VALUES
  (
    2,
    6,
    'Sarah Williams',
    'sarah.w@example.com',
    '(555) 777-8888',
    'Very interested in the 2019 BMW 3 Series. Can you provide the vehicle history report? What does the certified pre-owned warranty cover?'
  ),
  (
    2,
    9,
    'Michael Brown',
    'michael.brown@example.com',
    '(555) 999-0000',
    'I''d like more information about the Lexus RX 350. Has it been in any accidents? Can I bring my mechanic for an inspection?'
  );
```

## Running Scripts on Railway

**Method 1: Railway CLI (Recommended)**

```bash
# Install Railway CLI (one-time setup)
npm install -g @railway/cli

# Login to Railway
railway login

# Link to your project
railway link

# Run schema script
railway run psql $DATABASE_URL < backend/db/schema.sql

# Run seed script
railway run psql $DATABASE_URL < backend/db/seed.sql
```

**Method 2: Railway Dashboard**

1. Railway project → **PostgreSQL plugin** → **Data** tab → **Query**
2. Copy contents of `schema.sql`, paste, execute
3. Copy contents of `seed.sql`, paste, execute

**Verification:**

```bash
railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM dealership;"
# Expected: 2

railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM vehicle;"
# Expected: 10

railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM lead;"
# Expected: 5
```

---
