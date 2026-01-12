# 9. Source Tree

Complete file tree for 2-day MVP. Every file listed is needed. Use as implementation checklist.

## Root Directory

```
JealPrototypeTest/
├── backend/                      # Express API server
├── frontend/                     # React app (Vite)
├── .gitignore                    # Git ignore rules
├── package.json                  # Root workspace config
├── README.md                     # Setup instructions
└── .env.example                  # Environment variable template
```

## Root Files

**package.json** (Root Workspace)

```json
{
  "name": "jeal-prototype-test",
  "version": "1.0.0",
  "private": true,
  "workspaces": [
    "backend",
    "frontend"
  ],
  "scripts": {
    "dev": "concurrently \"npm run server\" \"npm run client\"",
    "server": "npm run dev --workspace=backend",
    "client": "npm run dev --workspace=frontend",
    "build": "npm run build --workspace=frontend",
    "start": "npm start --workspace=backend"
  },
  "devDependencies": {
    "concurrently": "^8.2.2"
  }
}
```

**.gitignore**

```
# Dependencies
node_modules/
package-lock.json

# Environment
.env
.env.local

# Build outputs
frontend/dist/
frontend/build/

# Logs
*.log
npm-debug.log*

# OS files
.DS_Store
Thumbs.db

# IDE
.vscode/
.idea/
*.swp
*.swo
```

**.env.example**

```bash
# Backend - Railway PostgreSQL (same for dev and production)
DATABASE_URL=postgresql://postgres:ABC123@containers-us-west-123.railway.app:6789/railway

# Session secret (generate random string)
SESSION_SECRET=your-random-secret-here-change-this
ADMIN_USERNAME=admin
ADMIN_PASSWORD=admin123
NODE_ENV=development
PORT=5000

# Cloudinary
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret

# Email Configuration (for lead notifications)
# Optional for development, required for production
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USER=your-email@gmail.com
EMAIL_PASSWORD=your-app-specific-password
EMAIL_FROM=noreply@yourdomain.com

# Frontend (Vite requires VITE_ prefix)
VITE_CLOUDINARY_CLOUD_NAME=your-cloud-name
VITE_CLOUDINARY_UPLOAD_PRESET=vehicle-images
```

**README.md**

```markdown
# Multi-Dealership Car Website + CMS Platform

2-day MVP prototype for multi-tenant dealership platform.

# Quick Start

1. **Clone and install:**
   ```bash
   git clone <repo-url>
   cd JealPrototypeTest
   npm install
   ```

2. **Set up Railway database:**
   - Create Railway account and PostgreSQL instance
   - Copy DATABASE_URL from Railway dashboard
   - Paste into .env file

3. **Set up environment:**
   ```bash
   cp .env.example .env
   # Edit .env with Railway DATABASE_URL, Cloudinary credentials, etc.
   ```

4. **Run database migrations:**
   ```bash
   railway login
   railway link
   railway run psql $DATABASE_URL < backend/db/schema.sql
   railway run psql $DATABASE_URL < backend/db/seed.sql
   ```

5. **Run development servers:**
   ```bash
   npm run dev
   ```
   - Backend: http://localhost:5000
   - Frontend: http://localhost:3000

6. **Login to admin:**
   - URL: http://localhost:3000/admin/login
   - Username: admin
   - Password: (from .env ADMIN_PASSWORD)

# Tech Stack

- Backend: Node.js 18, Express, node-postgres
- Frontend: React 18, Vite, Tailwind CSS, React Router
- Database: PostgreSQL 14+ (Railway)
- Image Storage: Cloudinary
- Deployment: Railway

# Project Structure

- `/backend` - Express API server
- `/frontend` - React SPA (public site + admin CMS)

# Deployment

Deploy to Railway:
1. Push to GitHub
2. Import project in Railway
3. Add PostgreSQL plugin
4. Set environment variables
5. Deploy
```

## Backend Structure

```
backend/
├── server.js                     # Express app setup, entry point
├── package.json                  # Backend dependencies
├── .env                          # Environment variables (not committed)
├── routes/
│   ├── auth.js                   # POST /auth/login, /logout, GET /auth/me
│   ├── dealers.js                # GET/PUT /dealers
│   ├── vehicles.js               # CRUD /vehicles
│   ├── leads.js                  # GET/POST /leads (with email notifications)
│   └── upload.js                 # POST /upload (Cloudinary fallback)
├── services/
│   └── emailService.js           # Email notification service (nodemailer)
├── db/
│   ├── index.js                  # PostgreSQL connection pool
│   ├── dealers.js                # Dealership queries
│   ├── vehicles.js               # Vehicle queries
│   ├── leads.js                  # Lead queries
│   ├── schema.sql                # Database schema (CREATE TABLE)
│   └── seed.sql                  # Seed data (2 dealerships + vehicles + leads)
└── middleware/
    └── auth.js                   # requireAuth middleware
```

**backend/package.json**

```json
{
  "name": "backend",
  "version": "1.0.0",
  "main": "server.js",
  "scripts": {
    "dev": "nodemon server.js",
    "start": "node server.js"
  },
  "dependencies": {
    "express": "^4.18.2",
    "pg": "^8.11.3",
    "dotenv": "^16.3.1",
    "express-session": "^1.17.3",
    "cookie-parser": "^1.4.6",
    "cors": "^2.8.5",
    "morgan": "^1.10.0",
    "cloudinary": "^2.8.0",
    "nodemailer": "^7.0.11",
    "multer": "^2.0.2"
  },
  "devDependencies": {
    "nodemon": "^3.0.2"
  }
}
```

**backend/server.js**

See Section 2 (High Level Architecture) for complete implementation.

**backend/db/index.js**

```javascript
const { Pool } = require('pg');

const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: process.env.NODE_ENV === 'production'
    ? { rejectUnauthorized: false }
    : false
});

// Test connection
pool.query('SELECT NOW()', (err, res) => {
  if (err) {
    console.error('Database connection error:', err);
  } else {
    console.log('Database connected successfully');
  }
});

module.exports = pool;
```

**backend/middleware/auth.js**

```javascript
function requireAuth(req, res, next) {
  if (!req.session.isAuthenticated) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}

module.exports = { requireAuth };
```

## Frontend Structure

```
frontend/
├── index.html                    # HTML entry point (with Cloudinary script)
├── package.json                  # Frontend dependencies
├── vite.config.js                # Vite configuration (proxy to backend)
├── tailwind.config.js            # Tailwind CSS configuration
├── postcss.config.js             # PostCSS configuration
├── .env                          # Frontend env vars (VITE_* prefix)
├── src/
│   ├── main.jsx                  # React entry point
│   ├── App.jsx                   # Root component with routes
│   ├── index.css                 # Tailwind directives + global styles
│   ├── pages/
│   │   ├── public/
│   │   │   ├── Home.jsx          # Public home page
│   │   │   ├── Inventory.jsx     # Vehicle listing with search/filter
│   │   │   ├── VehicleDetail.jsx # Vehicle detail + enquiry form
│   │   │   └── About.jsx         # Dealership about/contact page
│   │   └── admin/
│   │       ├── Login.jsx         # Admin login page
│   │       ├── Dashboard.jsx     # Admin dashboard with dealership selector
│   │       ├── VehicleList.jsx   # Vehicle manager (list + actions)
│   │       ├── VehicleForm.jsx   # Create/edit vehicle form
│   │       ├── DealerSettings.jsx # Dealership settings form
│   │       └── LeadInbox.jsx     # Customer enquiries list
│   ├── components/
│   │   ├── Layout.jsx            # Public site layout wrapper
│   │   ├── Header.jsx            # Public site header/nav with dealership selector
│   │   ├── AdminHeader.jsx       # Admin header with dealership selector + view website button
│   │   ├── DealershipSelector.jsx # Dropdown for selecting dealership (public site)
│   │   ├── VehicleCard.jsx       # Vehicle grid item component
│   │   ├── EnquiryForm.jsx       # Reusable enquiry form
│   │   ├── ImageGallery.jsx      # Vehicle detail image gallery
│   │   └── ProtectedRoute.jsx    # Auth wrapper for admin routes
│   ├── context/
│   │   ├── AdminContext.jsx      # Global admin state (auth, selected dealership)
│   │   └── DealershipContext.jsx # Global public site dealership selection state
│   ├── hooks/
│   │   └── useDealership.js      # Custom hook for fetching dealership data
│   └── utils/
│       └── api.js                # Fetch wrappers for API calls (optional)
└── public/
    └── (static assets like favicon.ico)
```

**frontend/package.json**

```json
{
  "name": "frontend",
  "version": "1.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.20.0",
    "react-hook-form": "^7.49.0"
  },
  "devDependencies": {
    "@vitejs/plugin-react": "^4.2.1",
    "vite": "^5.0.8",
    "tailwindcss": "^3.4.0",
    "postcss": "^8.4.32",
    "autoprefixer": "^10.4.16"
  }
}
```

**frontend/index.html**

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dealership Platform</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.jsx"></script>

    <!-- Cloudinary Upload Widget -->
    <script src="https://upload-widget.cloudinary.com/global/all.js" type="text/javascript"></script>
  </body>
</html>
```

**frontend/vite.config.js**

```javascript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  },
  build: {
    outDir: 'dist'
  }
});
```

**frontend/tailwind.config.js**

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,jsx}"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

**frontend/postcss.config.js**

```javascript
export default {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
```

**frontend/src/index.css**

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

/* Global styles */
body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Custom utility classes */
.btn-primary {
  @apply bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition;
}

.btn-secondary {
  @apply bg-gray-600 text-white px-4 py-2 rounded hover:bg-gray-700 transition;
}

.btn-danger {
  @apply bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 transition;
}

.input-field {
  @apply border border-gray-300 rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.card {
  @apply bg-white rounded-lg shadow-md p-4;
}
```

**frontend/src/main.jsx**

```javascript
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

**frontend/src/App.jsx**

See Section 6 (Components) for complete implementation.

## Total File Count

**Backend:** 14 files
- 1 server.js
- 1 package.json
- 5 route files
- 4 db files
- 2 SQL scripts
- 1 middleware file

**Frontend:** 27 files
- 1 index.html
- 1 package.json
- 3 config files (vite, tailwind, postcss)
- 3 root files (main.jsx, App.jsx, index.css)
- 10 page components
- 7 shared components
- 1 context file
- 1 utils file (optional)

**Root:** 4 files
- package.json, .gitignore, README.md, .env.example

**Grand Total:** ~45 files

---
