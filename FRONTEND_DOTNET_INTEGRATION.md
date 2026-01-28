# Frontend API Integration Guide

**Date:** 2026-01-27  
**Status:** Complete

---

## Overview

The frontend has been updated to communicate with the new .NET backend API running on port 5001. All API calls now go through a centralized utility function for consistency and maintainability.

---

## Configuration

### Environment Variables

**File:** `frontend/.env`

```env
# Backend API URL - .NET API runs on port 5001
VITE_API_URL=http://localhost:5001

# Google reCAPTCHA Site Key (test key for development)
VITE_RECAPTCHA_SITE_KEY=6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI
```

**For Production:**
```env
VITE_API_URL=https://api.yourdomain.com
VITE_RECAPTCHA_SITE_KEY=your_production_key_here
```

---

## API Utility

### Location
`frontend/src/utils/api.js`

### Usage

```javascript
import apiRequest from '../utils/api';

// Simple GET request
const response = await apiRequest('/api/dealers');
const data = await response.json();

// POST request with body
const response = await apiRequest('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify({ username, password })
});

// PUT request
const response = await apiRequest(`/api/vehicles/${id}`, {
  method: 'PUT',
  body: JSON.stringify(vehicleData)
});

// DELETE request
const response = await apiRequest(`/api/vehicles/${id}`, {
  method: 'DELETE'
});
```

### Features

- ✅ Automatic base URL prepending (`http://localhost:5001`)
- ✅ Credentials included automatically (session cookies)
- ✅ Content-Type: application/json by default
- ✅ Environment-based URL configuration
- ✅ Single point of maintenance for API changes

---

## Updated Files (25 total)

### Core Context & Hooks
1. `context/AdminContext.jsx` - Authentication state
2. `hooks/useDealership.js` - Dealership data hook

### Admin Pages (10 files)
3. `pages/admin/Login.jsx`
4. `pages/admin/Dashboard.jsx`
5. `pages/admin/DealershipManagement.jsx`
6. `pages/admin/DealerSettings.jsx`
7. `pages/admin/UserManagement.jsx`
8. `pages/admin/VehicleList.jsx`
9. `pages/admin/VehicleForm.jsx`
10. `pages/admin/LeadInbox.jsx`
11. `pages/admin/SalesRequests.jsx`
12. `pages/admin/BlogList.jsx`
13. `pages/admin/BlogForm.jsx`

### Public Pages (5 files)
14. `pages/public/Inventory.jsx`
15. `pages/public/VehicleDetail.jsx`
16. `pages/public/Blog.jsx`
17. `pages/public/BlogPost.jsx`
18. `pages/public/SellYourCar.jsx`

### Components (8 files)
19. `components/AdminHeader.jsx`
20. `components/DealershipSelector.jsx`
21. `components/EnquiryForm.jsx`
22. `components/GeneralEnquiryForm.jsx`
23. `components/GoogleReviewsCarousel.jsx`
24. `components/admin/NavigationManager.jsx`
25. `components/admin/TemplateSelector.jsx`

---

## Migration Pattern

### Before (Direct fetch)
```javascript
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include',
  body: JSON.stringify({ username, password })
});
```

### After (Using apiRequest)
```javascript
import apiRequest from '../utils/api';

const response = await apiRequest('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify({ username, password })
});
```

**Changes:**
- ✅ Import `apiRequest` utility
- ✅ Replace `fetch()` with `apiRequest()`
- ✅ Remove `credentials: 'include'` (automatic)
- ✅ Remove `headers` (automatic)
- ✅ Keep method and body as-is

---

## API Endpoints

All endpoints remain the same, now pointing to .NET backend:

### Authentication
- `POST /api/auth/login` - User login
- `GET /api/auth/me` - Get current user
- `POST /api/auth/logout` - User logout

### Dealerships
- `GET /api/dealers` - List all dealerships
- `GET /api/dealers/:id` - Get dealership by ID
- `GET /api/dealers/url/:websiteUrl` - Get dealership by URL
- `POST /api/dealers` - Create dealership
- `PUT /api/dealers/:id` - Update dealership
- `DELETE /api/dealers/:id` - Delete dealership

### Vehicles
- `GET /api/vehicles` - List all vehicles (with filters)
- `GET /api/vehicles/:id` - Get vehicle by ID
- `GET /api/dealers/:dealershipId/vehicles` - Get dealership vehicles
- `POST /api/vehicles` - Create vehicle
- `PUT /api/vehicles/:id` - Update vehicle
- `DELETE /api/vehicles/:id` - Delete vehicle

### Users
- `GET /api/users` - List all users
- `GET /api/users/:id` - Get user by ID
- `GET /api/dealers/:dealershipId/users` - Get dealership users
- `POST /api/users` - Create user
- `PUT /api/users/:id` - Update user
- `DELETE /api/users/:id` - Delete user

### Leads
- `GET /api/leads` - List all leads
- `POST /api/leads` - Create lead (vehicle enquiry)
- `PUT /api/leads/:id/status` - Update lead status

### Sales Requests
- `GET /api/sales-requests` - List all sales requests
- `POST /api/sales-requests` - Create sales request (sell your car)
- `PUT /api/sales-requests/:id/status` - Update sales request status

### Blog Posts
- `GET /api/blog-posts` - List all blog posts
- `GET /api/blog-posts/:id` - Get blog post by ID
- `POST /api/blog-posts` - Create blog post
- `PUT /api/blog-posts/:id` - Update blog post
- `DELETE /api/blog-posts/:id` - Delete blog post

### Design Templates
- `GET /api/design-templates/:dealershipId` - Get design template
- `PUT /api/design-templates/:dealershipId` - Update design template

### Google Reviews
- `GET /api/google-reviews/:dealershipId` - Get Google reviews

---

## Testing

### 1. Start .NET Backend
```bash
cd backend-dotnet/JealPrototype.API
dotnet run
```
**Expected:** API running on `http://localhost:5001`

### 2. Start Frontend
```bash
cd frontend
npm run dev
```
**Expected:** Frontend running on `http://localhost:3000`

### 3. Test Authentication
1. Navigate to `http://localhost:3000/admin/login`
2. Login with test credentials
3. Verify successful authentication
4. Check browser Network tab for requests to `http://localhost:5001`

### 4. Test API Integration
- ✅ Login/Logout
- ✅ Dashboard data loading
- ✅ Dealership management
- ✅ Vehicle management
- ✅ User management
- ✅ Lead creation
- ✅ Sales request creation
- ✅ Blog post management

---

## Troubleshooting

### CORS Errors
**Problem:** `Access to fetch at 'http://localhost:5001/api/...' from origin 'http://localhost:3000' has been blocked by CORS`

**Solution:** Verify CORS configuration in `.NET API Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

app.UseCors("AllowFrontend");
```

### 404 Not Found
**Problem:** `GET http://localhost:5001/api/dealers 404 (Not Found)`

**Solution:** 
1. Verify .NET API is running on port 5001
2. Check API route in controller: `[Route("api/[controller]")]`
3. Verify endpoint URL matches controller name

### Connection Refused
**Problem:** `Failed to fetch` or `net::ERR_CONNECTION_REFUSED`

**Solution:**
1. Ensure .NET API is running: `dotnet run`
2. Verify port 5001 is not blocked by firewall
3. Check `VITE_API_URL` in `.env` file

### Session/Auth Issues
**Problem:** User logged out immediately or auth state lost

**Solution:**
1. Verify `credentials: 'include'` in apiRequest (automatic)
2. Check cookie settings in .NET API
3. Ensure CORS allows credentials: `.AllowCredentials()`

---

## Environment-Specific Configuration

### Development
```env
VITE_API_URL=http://localhost:5001
```

### Staging
```env
VITE_API_URL=https://api-staging.yourdomain.com
```

### Production
```env
VITE_API_URL=https://api.yourdomain.com
```

**Note:** Update `.env.production` for production builds

---

## Best Practices

1. ✅ **Always use `apiRequest()`** - Never use `fetch()` directly for API calls
2. ✅ **Keep endpoint paths consistent** - Use `/api/...` format
3. ✅ **Handle errors gracefully** - Check `response.ok` before parsing JSON
4. ✅ **Use environment variables** - Never hardcode API URLs
5. ✅ **Test in all environments** - Dev, staging, production

---

## Migration Checklist

- [x] Created `utils/api.js` utility
- [x] Updated `.env` with `VITE_API_URL`
- [x] Updated all context files
- [x] Updated all admin pages
- [x] Updated all public pages
- [x] Updated all components
- [x] Updated all hooks
- [x] Tested basic functionality
- [ ] End-to-end integration testing
- [ ] Performance testing
- [ ] Production deployment

---

**Last Updated:** 2026-01-27  
**Status:** ✅ Frontend Integration Complete  
**Next:** Database configuration and end-to-end testing
