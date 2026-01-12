-- ============================================================================
-- Database Seed Data for Multi-Dealership Car Website Platform
-- ============================================================================
-- This script populates the database with sample data for testing:
-- - 2 dealerships (Acme Auto Sales, Premier Motors)
-- - 20 vehicles (10 per dealership) with varied types, statuses, prices (Story 1.7)
-- - 5 leads (3 for dealership 1, 2 for dealership 2)
--
-- Run this script after schema.sql via Docker (local development):
-- docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\seed.sql
--
-- Story 1.7 Enhancement: Added 5 additional vehicles per dealership for frontend testing
-- Variety includes: sedans, SUVs, trucks, luxury vehicles with mixed statuses
-- ============================================================================

-- ============================================================================
-- DEALERSHIP DATA
-- ============================================================================

INSERT INTO dealership (name, logo_url, address, phone, email, hours, about) VALUES
(
  'Acme Auto Sales',
  NULL,
  '123 Main St, Springfield, IL 62701',
  '(555) 123-4567',
  'sales@acmeauto.com',
  'Mon-Fri 9am-6pm, Sat 10am-4pm, Sun Closed',
  'Family-owned dealership serving Springfield for over 25 years. We specialize in quality used vehicles with comprehensive warranties and financing options for all credit levels. Our experienced team is committed to finding you the perfect vehicle at the right price.'
),
(
  'Premier Motors',
  NULL,
  '456 Oak Ave, Springfield, IL 62702',
  '(555) 987-6543',
  'info@premiermotors.com',
  'Mon-Sat 9am-7pm, Sun 11am-5pm',
  'Premier luxury and performance vehicle dealership offering the finest selection of premium automobiles. Our state-of-the-art facility features an extensive inventory of high-end vehicles, certified service center, and exclusive financing packages tailored for discerning buyers.'
);

-- ============================================================================
-- VEHICLE DATA - DEALERSHIP 1 (Acme Auto Sales)
-- ============================================================================

INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
(
  1,
  'Toyota',
  'Camry',
  2021,
  24995.00,
  32000,
  'used',
  'active',
  '2021 Toyota Camry SE - Low Miles, Excellent Condition',
  'Well-maintained Toyota Camry SE with only 32k miles. Features include backup camera, lane departure warning, adaptive cruise control, and Toyota Safety Sense. Single owner, non-smoker, clean CarFax. Recently serviced with new tires. Perfect commuter car with legendary Toyota reliability.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/camry-front.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/camry-interior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/camry-side.jpg"]'::jsonb
),
(
  1,
  'Honda',
  'CR-V',
  2020,
  27500.00,
  45000,
  'used',
  'active',
  '2020 Honda CR-V EX - Family SUV with All-Wheel Drive',
  'Spacious Honda CR-V EX with AWD, perfect for families. Loaded with Honda Sensing safety suite, power liftgate, sunroof, and heated seats. Excellent fuel economy for an SUV. Two owners, complete maintenance records available. Ready for your next adventure.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/crv-exterior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/crv-cargo.jpg"]'::jsonb
),
(
  1,
  'Ford',
  'F-150',
  2019,
  32900.00,
  58000,
  'used',
  'pending',
  '2019 Ford F-150 XLT SuperCrew - Work Truck Ready',
  'Rugged F-150 XLT with 5.0L V8 engine and 4WD. SuperCrew cab with spacious seating for 5. Towing package rated for 11,000 lbs. Bed liner, running boards, and tonneau cover included. Fleet-maintained with detailed service history. Sale pending - similar trucks available.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/f150-profile.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/f150-bed.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/f150-interior.jpg"]'::jsonb
),
(
  1,
  'Mazda',
  'CX-5',
  2022,
  29995.00,
  18000,
  'used',
  'active',
  '2022 Mazda CX-5 Touring - Premium Compact SUV',
  'Nearly new Mazda CX-5 Touring with only 18k miles. Premium features include leather seats, Bose sound system, adaptive headlights, and full safety suite. Known for sporty handling and upscale interior. Still under factory warranty. One owner, garage kept.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/cx5-front.jpg"]'::jsonb
),
(
  1,
  'Chevrolet',
  'Silverado 1500',
  2018,
  28500.00,
  72000,
  'used',
  'sold',
  '2018 Chevrolet Silverado 1500 LT - SOLD',
  'SOLD - Popular Silverado 1500 with 5.3L V8 and Z71 off-road package. Double cab configuration with factory lift and all-terrain tires. Excellent towing capacity and off-road capability. This vehicle has been sold, but we have similar trucks in stock.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/silverado-front.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/silverado-interior.jpg"]'::jsonb
),
-- Additional Vehicles for Dealership 1 (Acme Auto Sales) - Story 1.7 Enhancement
(
  1,
  'Nissan',
  'Altima',
  2020,
  19995.00,
  48000,
  'used',
  'active',
  '2020 Nissan Altima SR - Reliable Sedan',
  'Dependable Nissan Altima SR with sporty styling and advanced safety features. Includes ProPILOT Assist, blind spot monitoring, and remote start. Great fuel economy with comfortable ride quality. Clean history, well-maintained. Perfect for daily commuting.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/altima-front.jpg"]'::jsonb
),
(
  1,
  'Jeep',
  'Wrangler',
  2021,
  34500.00,
  38000,
  'used',
  'active',
  '2021 Jeep Wrangler Unlimited Sport - Adventure Ready',
  'Iconic Jeep Wrangler Unlimited 4-door with removable top and doors. 4WD with upgraded off-road tires and rock rails. Perfect for weekend adventures and daily driving. Never seen serious off-road use, garage kept. Includes hardtop and soft top.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/wrangler-exterior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/wrangler-interior.jpg"]'::jsonb
),
(
  1,
  'Subaru',
  'Outback',
  2019,
  26900.00,
  42000,
  'used',
  'pending',
  '2019 Subaru Outback Premium - All-Weather Wagon',
  'Versatile Subaru Outback with legendary AWD and spacious cargo area. EyeSight driver assist, X-Mode for enhanced traction, and roof rails. Excellent safety ratings. Well-maintained with complete service records. Sale pending - contact for similar vehicles.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/outback-side.jpg"]'::jsonb
),
(
  1,
  'Hyundai',
  'Elantra',
  2022,
  17500.00,
  35000,
  'used',
  'active',
  '2022 Hyundai Elantra SE - Affordable & Efficient',
  'Nearly new Hyundai Elantra with excellent warranty coverage remaining. Modern styling with impressive fuel economy (up to 40+ mpg highway). Apple CarPlay/Android Auto, forward collision avoidance, and lane keep assist. One owner, non-smoker. Great first car or budget-friendly commuter.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/elantra-front.jpg"]'::jsonb
),
(
  1,
  'Ram',
  '1500',
  2017,
  31900.00,
  65000,
  'used',
  'draft',
  '2017 Ram 1500 Big Horn Crew Cab - Coming Soon',
  'DRAFT LISTING - Powerful Ram 1500 with 5.7L HEMI V8 arriving from trade-in. Crew cab with luxury interior, heated seats, and upgraded infotainment. Tow package rated for 10,000 lbs. Currently undergoing inspection and detailing. Available next week.',
  '[]'::jsonb
);

-- ============================================================================
-- VEHICLE DATA - DEALERSHIP 2 (Premier Motors)
-- ============================================================================

INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
(
  2,
  'BMW',
  '330i',
  2022,
  42995.00,
  15000,
  'used',
  'active',
  '2022 BMW 330i xDrive - Luxury Sport Sedan',
  'Immaculate BMW 330i with xDrive all-wheel drive. Turbocharged 255hp engine delivers thrilling performance. Premium package includes heated leather seats, navigation, Harman Kardon audio, and wireless charging. Certified pre-owned with extended warranty. Meticulously maintained, showroom condition.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/bmw330-exterior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/bmw330-interior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/bmw330-dash.jpg"]'::jsonb
),
(
  2,
  'Mercedes-Benz',
  'E-Class',
  2021,
  54900.00,
  22000,
  'used',
  'active',
  '2021 Mercedes-Benz E 350 4MATIC - Executive Sedan',
  'Stunning E-Class with luxurious interior and cutting-edge technology. Features include panoramic sunroof, premium Burmester sound system, massage seats, adaptive suspension, and full ADAS suite. Impeccably maintained with complete service history. Perfect blend of comfort and performance.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/eclass-front.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/eclass-profile.jpg"]'::jsonb
),
(
  2,
  'Audi',
  'Q5',
  2023,
  48500.00,
  8500,
  'used',
  'active',
  '2023 Audi Q5 Premium Plus - Modern Luxury SUV',
  'Like-new Audi Q5 Premium Plus with Quattro AWD and virtual cockpit. Loaded with Bang & Olufsen audio, panoramic roof, matrix LED headlights, and advanced driver assistance. Only 8,500 miles with full factory warranty remaining. Sophisticated design meets cutting-edge technology.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/q5-exterior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/q5-interior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/q5-cockpit.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/q5-rear.jpg"]'::jsonb
),
(
  2,
  'Lexus',
  'RX 350',
  2022,
  51995.00,
  12000,
  'used',
  'draft',
  '2022 Lexus RX 350 F Sport - Premium SUV (Coming Soon)',
  'DRAFT LISTING - Pristine Lexus RX 350 F Sport arriving next week. Features adaptive variable suspension, Mark Levinson audio, triple-beam LED headlights, and legendary Lexus reliability. Full inspection and detailing in progress. Contact us for early viewing appointment.',
  '[]'::jsonb
),
(
  2,
  'Porsche',
  'Macan',
  2021,
  62500.00,
  18000,
  'used',
  'pending',
  '2021 Porsche Macan S - Performance SUV',
  'Exhilarating Macan S with 375hp twin-turbo V6 engine. Sport Chrono package, air suspension, Bose audio, and 20-inch wheels. Porsche Active Suspension Management delivers superb handling. Certified pre-owned with 2-year warranty. Sale pending - test drives available for qualified buyers.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/macan-front.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/macan-interior.jpg"]'::jsonb
),
-- Additional Vehicles for Dealership 2 (Premier Motors) - Story 1.7 Enhancement
(
  2,
  'Tesla',
  'Model 3',
  2021,
  38900.00,
  22000,
  'used',
  'active',
  '2021 Tesla Model 3 Long Range - Electric Performance',
  'Stunning Tesla Model 3 Long Range with AWD. Over 350 miles of range on a single charge. Autopilot, premium interior, 15-inch touchscreen, and over-the-air updates. Acceleration 0-60 in 4.2 seconds. Free Supercharging included. Minimal maintenance required.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/tesla-model3-front.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/tesla-model3-interior.jpg"]'::jsonb
),
(
  2,
  'Cadillac',
  'Escalade',
  2020,
  67500.00,
  28000,
  'used',
  'active',
  '2020 Cadillac Escalade Luxury - Ultimate Full-Size SUV',
  'Commanding Cadillac Escalade with 6.2L V8 engine and luxurious 7-passenger interior. Magnetic Ride Control, adaptive cruise, 360-degree camera, and premium AKG audio system. Perfect for executive transport or family adventures. Impeccably maintained, all service records available.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/escalade-exterior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/escalade-interior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/escalade-cargo.jpg"]'::jsonb
),
(
  2,
  'Infiniti',
  'Q50',
  2019,
  32995.00,
  41000,
  'used',
  'active',
  '2019 Infiniti Q50 Red Sport 400 - Performance Luxury Sedan',
  'Powerful Infiniti Q50 Red Sport 400 with twin-turbo V6 delivering 400hp. Sport-tuned suspension, steer-by-wire technology, and premium Bose audio. Luxurious leather interior with advanced driver assistance features. Excellent balance of performance and comfort.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/q50-front.jpg"]'::jsonb
),
(
  2,
  'Volvo',
  'XC90',
  2020,
  44900.00,
  35000,
  'used',
  'sold',
  '2020 Volvo XC90 T6 Momentum - SOLD',
  'SOLD - Premium Volvo XC90 with renowned safety features and Scandinavian design. 7-passenger seating, Pilot Assist semi-autonomous driving, panoramic sunroof, and Bowers & Wilkins audio. This vehicle sold quickly - contact us for similar luxury SUVs in our inventory.',
  '["https://res.cloudinary.com/demo/image/upload/v1/cars/xc90-exterior.jpg", "https://res.cloudinary.com/demo/image/upload/v1/cars/xc90-interior.jpg"]'::jsonb
),
(
  2,
  'Genesis',
  'G70',
  2022,
  36500.00,
  19000,
  'used',
  'draft',
  '2022 Genesis G70 3.3T Sport - Arriving Soon',
  'DRAFT LISTING - Exceptional Genesis G70 3.3T Sport with twin-turbo V6. Sport-tuned suspension, Brembo brakes, and luxurious interior rivaling German competitors. Industry-leading warranty coverage. Currently undergoing certification process. Available for viewing next week.',
  '[]'::jsonb
);

-- ============================================================================
-- LEAD DATA - DEALERSHIP 1 (Acme Auto Sales)
-- ============================================================================

INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message) VALUES
(
  1,
  1,
  'Sarah Johnson',
  'sarah.johnson@email.com',
  '(555) 234-5678',
  'Hi, I am interested in the 2021 Toyota Camry. Is it still available? I would like to schedule a test drive this weekend if possible. Also, do you offer financing options? Thanks!'
),
(
  1,
  2,
  'Michael Chen',
  'mchen@email.com',
  '(555) 345-6789',
  'I saw the Honda CR-V on your website and it looks perfect for my family. Can you provide the CarFax report? Also interested in your warranty options. When is the best time to come see it?'
),
(
  1,
  NULL,
  'Robert Williams',
  'rwilliams88@email.com',
  '(555) 456-7890',
  'Looking for a reliable used truck under $35k. Do you have any Ford or Chevy trucks available that match this criteria? I need something with good towing capacity for work. Please call me back at your earliest convenience.'
);

-- ============================================================================
-- LEAD DATA - DEALERSHIP 2 (Premier Motors)
-- ============================================================================

INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message) VALUES
(
  2,
  6,
  'Jennifer Martinez',
  'j.martinez@email.com',
  '(555) 567-8901',
  'Very interested in the BMW 330i. I currently drive a 2018 3-series and would like to upgrade. What is your best price on this vehicle? Do you accept trade-ins? I can come by tomorrow afternoon to discuss.'
),
(
  2,
  8,
  'David Thompson',
  'dthompson@email.com',
  '(555) 678-9012',
  'The Audi Q5 Premium Plus is exactly what I have been looking for. Can you tell me more about the remaining warranty coverage? Also, I would like to know about your financing rates. I have excellent credit and can provide a substantial down payment.'
);

-- ============================================================================
-- SEED DATA COMPLETE
-- ============================================================================
-- Verify data insertion:
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM dealership;"  # Expected: 2
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM vehicle;"     # Expected: 20 (Story 1.7: Increased from 10 to 20)
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT COUNT(*) FROM lead;"        # Expected: 5
--
-- View sample data:
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name FROM dealership;"
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, dealership_id, make, model, status FROM vehicle;"
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT dealership_id, COUNT(*) FROM vehicle GROUP BY dealership_id;"  # Expected: 10 per dealership
--
-- Story 1.7 Enhancement Summary:
-- - Dealership 1 (Acme Auto Sales): 10 vehicles (5 original + 5 new)
--   Mix of sedans, SUVs, trucks with statuses: 6 active, 2 pending, 1 sold, 1 draft
--   Price range: $17,500 - $34,500
-- - Dealership 2 (Premier Motors): 10 luxury vehicles (5 original + 5 new)
--   Mix of luxury sedans, SUVs, electric vehicles with statuses: 6 active, 2 pending, 1 sold, 1 draft
--   Price range: $32,995 - $67,500
-- ============================================================================
