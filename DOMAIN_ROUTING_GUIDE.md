# Domain-Based Routing - Implementation Guide

**Making Website URLs Actually Work**

---

## Current State

The website URL field **stores** URLs but doesn't **route** to them yet.

When you set `acme-auto-test.com` in the admin panel:
- ✅ URL is saved in database
- ✅ URL is displayed in admin panel
- ❌ Domain doesn't automatically route to your dealership

---

## What You Need for Real Domain Routing

### Prerequisites

1. **Own the Domain**
   - Register domain with a registrar (GoDaddy, Namecheap, etc.)
   - Cost: ~$10-15/year per domain

2. **DNS Configuration**
   - Point domain to your server's IP address
   - Configure A record: `acme-auto-test.com` → `your.server.ip`

3. **Server Configuration**
   - Install SSL certificate for HTTPS
   - Configure web server (nginx/Apache) or use Node.js directly

4. **Backend Routing Logic**
   - Detect incoming domain
   - Look up dealership by website_url
   - Serve correct dealership data

---

## Implementation Steps

### Step 1: Add Database Function to Look Up by URL

**File:** `backend/db/dealers.js`

```javascript
/**
 * Retrieves a dealership by website URL.
 *
 * @param {string} websiteUrl - The website URL to search for
 * @returns {Promise<Object|null>} Dealership object or null if not found
 */
async function getByWebsiteUrl(websiteUrl) {
  const query = 'SELECT * FROM dealership WHERE website_url = $1';
  const result = await pool.query(query, [websiteUrl]);
  return result.rows[0] || null;
}

module.exports = {
  getById,
  update,
  getAll,
  create,
  deleteDealership,
  getByWebsiteUrl  // Add this export
};
```

### Step 2: Add Middleware to Detect Domain

**File:** `backend/middleware/domainDetection.js` (CREATE NEW FILE)

```javascript
/**
 * Middleware to detect dealership from incoming domain.
 * Sets req.dealershipId based on hostname.
 */
const dealersDb = require('../db/dealers');

async function detectDealership(req, res, next) {
  try {
    // Get hostname from request
    const hostname = req.hostname; // e.g., 'acme-auto-test.com'
    
    // Skip localhost (development mode)
    if (hostname === 'localhost' || hostname.startsWith('127.0.0.1')) {
      return next();
    }
    
    // Look up dealership by website_url
    const dealership = await dealersDb.getByWebsiteUrl(hostname);
    
    if (dealership) {
      // Attach dealership to request
      req.detectedDealership = dealership;
      req.dealershipId = dealership.id;
    }
    
    next();
  } catch (error) {
    console.error('Domain detection error:', error);
    next(); // Continue even if detection fails
  }
}

module.exports = detectDealership;
```

### Step 3: Apply Middleware in Server

**File:** `backend/server.js`

```javascript
const express = require('express');
const detectDealership = require('./middleware/domainDetection');

const app = express();

// ... other middleware ...

// Domain detection middleware (add before routes)
app.use(detectDealership);

// Mount API routes
app.use('/api/dealers', dealersRouter);
// ... other routes ...
```

### Step 4: Update Frontend to Use Detected Dealership

**File:** `frontend/src/context/DealershipContext.jsx`

```javascript
export function DealershipProvider({ children }) {
  const [currentDealershipId, setCurrentDealershipId] = useState(() => {
    // Check if dealership was detected from domain
    const detected = window.dealershipId; // Set by backend
    if (detected) return detected;
    
    // Fallback to localStorage
    const saved = localStorage.getItem('selectedDealershipId');
    return saved ? parseInt(saved, 10) : 1;
  });
  
  // ... rest of component
}
```

### Step 5: Pass Detected Dealership to Frontend

**File:** `backend/server.js`

```javascript
// Serve frontend (modify existing code)
app.get('*', (req, res) => {
  // Inject dealership ID if detected
  if (req.detectedDealership) {
    // Send HTML with injected script
    res.send(`
      <!DOCTYPE html>
      <html>
        <head>
          <script>
            window.dealershipId = ${req.detectedDealership.id};
          </script>
        </head>
        <body>
          <div id="root"></div>
          <script src="/dist/bundle.js"></script>
        </body>
      </html>
    `);
  } else {
    // Serve normal frontend
    res.sendFile(path.join(__dirname, '../frontend/dist/index.html'));
  }
});
```

---

## DNS Configuration

### For Each Domain

1. **Log into your domain registrar**
2. **Add A Record:**
   ```
   Type: A
   Name: @ (or leave empty for root domain)
   Value: YOUR_SERVER_IP_ADDRESS
   TTL: 3600
   ```

3. **Add www subdomain (optional):**
   ```
   Type: CNAME
   Name: www
   Value: acme-auto-test.com
   TTL: 3600
   ```

4. **Wait for DNS propagation** (15 minutes - 48 hours)

### Check DNS Configuration

```bash
# Check if domain points to your server
nslookup acme-auto-test.com

# Or use dig
dig acme-auto-test.com +short
```

---

## SSL/HTTPS Configuration

### Using Let's Encrypt (Free)

```bash
# Install Certbot
sudo apt-get install certbot

# Get certificate for domain
sudo certbot certonly --standalone -d acme-auto-test.com

# Certificate will be saved to:
# /etc/letsencrypt/live/acme-auto-test.com/
```

### Configure Express for HTTPS

**File:** `backend/server.js`

```javascript
const https = require('https');
const fs = require('fs');

// HTTPS configuration
const httpsOptions = {
  key: fs.readFileSync('/etc/letsencrypt/live/acme-auto-test.com/privkey.pem'),
  cert: fs.readFileSync('/etc/letsencrypt/live/acme-auto-test.com/fullchain.pem')
};

// Create HTTPS server
const PORT = process.env.PORT || 443;
https.createServer(httpsOptions, app).listen(PORT, () => {
  console.log(`HTTPS server running on port ${PORT}`);
});
```

---

## Testing

### Local Testing (Without Real Domain)

**Edit your hosts file:**

**Windows:** `C:\Windows\System32\drivers\etc\hosts`  
**Mac/Linux:** `/etc/hosts`

Add line:
```
127.0.0.1  acme-auto-test.com
```

Now `acme-auto-test.com` will resolve to localhost for testing!

### Test the Flow

1. Start backend: `npm start`
2. Visit: `http://acme-auto-test.com:5000`
3. Should automatically detect and load Acme Auto Sales dealership

---

## Production Checklist

- [ ] Register domain names for each dealership
- [ ] Configure DNS A records pointing to server IP
- [ ] Wait for DNS propagation
- [ ] Install SSL certificates
- [ ] Implement backend routing logic
- [ ] Update frontend to use detected dealership
- [ ] Test with hosts file first
- [ ] Deploy to production server
- [ ] Test all domains
- [ ] Set up auto-renewal for SSL certificates

---

## Alternative: Subdomain Strategy

Instead of separate domains, use subdomains:

**Example:**
- Main site: `yourdealerships.com`
- Acme Auto: `acme.yourdealerships.com`
- Premier Motors: `premier.yourdealerships.com`

**Benefits:**
- Only need to buy one domain
- Wildcard SSL certificate covers all subdomains
- Easier DNS management

**DNS Configuration:**
```
Type: A
Name: *
Value: YOUR_SERVER_IP
TTL: 3600
```

This wildcard record makes ALL subdomains point to your server!

---

## Cost Estimate

### Separate Domains Approach
- Domain registration: $10-15/year per dealership
- SSL certificate: FREE (Let's Encrypt)
- Server: Existing cost
- **Total:** $10-15/year per dealership

### Subdomain Approach
- One domain: $10-15/year total
- Wildcard SSL: FREE (Let's Encrypt)
- Server: Existing cost
- **Total:** $10-15/year for ALL dealerships

---

## Current Workaround (No Code Changes)

**For now, to view different dealership websites:**

1. **Admin Panel Method:**
   - Login to admin panel
   - Use dealership selector dropdown
   - Switch between dealerships

2. **URL Parameter Method:**
   - `http://localhost:3000?dealership=1` (Acme Auto Sales)
   - `http://localhost:3000?dealership=2` (Premier Motors)

3. **LocalStorage Method:**
   ```javascript
   // In browser console:
   localStorage.setItem('selectedDealershipId', '1');
   location.reload();
   ```

---

## Summary

**What you have NOW:**
- ✅ Database field to store URLs
- ✅ Admin UI to manage URLs
- ✅ Foundation for future routing

**What you need for REAL routing:**
- ❌ Own the domain names
- ❌ Configure DNS records
- ❌ Implement backend routing logic
- ❌ Set up SSL certificates

**Recommendation:**
- Keep using current system (dealership selector) for development
- Implement domain routing when ready to go to production
- Consider subdomain strategy to save costs

---

**End of Guide**
