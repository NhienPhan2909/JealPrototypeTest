# Website URL Management - Quick Start Guide

**Quick Reference** for setting up unique website URLs for dealerships.

---

## 🚀 Quick Setup (3 Steps)

### Step 1: Run Database Migration
```bash
node backend\db\migrations\run_website_url_migration.js
```

Expected output:
```
✅ Migration completed successfully!
```

### Step 2: Log in as System Administrator
- Email: `admin@example.com`
- Password: `admin123`

### Step 3: Set Website URL
**Option A - For Existing Dealership:**
1. Go to **Admin Panel → Settings**
2. Find "Website URL" field
3. Enter URL (e.g., `acme-auto.com`)
4. Click "Update Dealership Settings"

**Option B - For New Dealership:**
1. Go to **Admin Panel → Dealerships**
2. Click "Create New Dealership"
3. Fill required fields + Website URL
4. Click "Create Dealership"

---

## 📋 Key Points

✅ **Only System Administrators** can manage website URLs  
✅ **Each URL must be unique** across all dealerships  
✅ **Optional field** - can be left empty  
✅ **No format restrictions** - any string up to 255 characters  

---

## 🎯 Common Use Cases

### Example 1: Set URL for Main Dealership
```
Dealership Name: Acme Auto Sales
Website URL: acme-auto.com
```

### Example 2: Multiple Locations
```
Dealership 1: Premium Motors North
Website URL: premium-motors-north.com

Dealership 2: Premium Motors South  
Website URL: premium-motors-south.com
```

### Example 3: Subdomain Strategy
```
Dealership 1: Main Location
Website URL: www.dealership.com

Dealership 2: Branch Location
Website URL: branch.dealership.com
```

---

## ⚡ Quick Commands

**Check if migration ran:**
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT column_name FROM information_schema.columns WHERE table_name = 'dealership' AND column_name = 'website_url';"
```

**View all dealership URLs:**
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, website_url FROM dealership ORDER BY id;"
```

**Set URL via SQL (if needed):**
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "UPDATE dealership SET website_url = 'acme-auto.com' WHERE id = 1;"
```

---

## 🐛 Troubleshooting

**Q: Migration says "already exists"**  
A: Already ran successfully. Check with: `SELECT website_url FROM dealership LIMIT 1;`

**Q: Error "duplicate key value"**  
A: URL already used by another dealership. Choose different URL.

**Q: Field not visible in admin panel**  
A: Clear browser cache, ensure logged in as System Administrator.

**Q: Can I use special characters?**  
A: Yes, but stick to valid URL characters for best results.

---

## 📊 Where to Find It

### Settings Page (Edit Existing)
```
Admin Panel → Settings → Website URL field
```

### Management Page (View/Create)
```
Admin Panel → Dealerships → 
  - Create form has Website URL field
  - List table shows Website URL column
```

---

## 🔗 Related Docs

- Full documentation: `WEBSITE_URL_FEATURE.md`
- Dealership management: `DEALERSHIP_MANAGEMENT_FEATURE.md`

---

**That's it! Your dealerships now have unique URLs. 🎉**
