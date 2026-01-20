# Dealership Management - Quick Start Guide

## For System Administrators

### How to Create a New Dealership

1. **Log in to Admin Panel**
   - URL: `http://localhost:5173/admin/login`
   - Username: `admin`
   - Password: `admin123`

2. **Navigate to Dealership Management**
   - Click **"Dealership Management"** in the top navigation menu
   - This link is only visible to System Administrators

3. **View Existing Dealerships**
   - See a table listing all dealerships
   - Columns: ID, Name, Website URL, Email, Phone, Created Date
   - **Click column headers to sort**: ID, Name, or Created Date
   - Look for arrows (↑/↓) showing current sort direction

4. **Create New Dealership**
   - Click **"+ Create New Dealership"** button (top right)
   - A modal form will appear

5. **Fill in Dealership Information**
   
   **Required Fields:**
   - **Dealership Name**: e.g., "Acme Auto Sales"
   - **Address**: Full street address with city, state, and ZIP
   - **Phone**: e.g., "(555) 123-4567"
   - **Email**: Contact email address
   
   **Optional Fields:**
   - **Website URL**: Custom URL/domain (e.g., "acme-auto.com")
   - **Logo URL**: Direct link to logo image (can upload via Dealership Settings later)
   - **Business Hours**: Multi-line text describing operating hours
   - **About**: Description of the dealership

6. **Submit the Form**
   - Click **"Create Dealership"** button
   - If successful, you'll see a green success message
   - The new dealership appears in the list

7. **Next Steps After Creating**
   - Go to **User Management** to create a dealership owner account
   - The owner can then configure their dealership and add content

## What Happens After Creation

When you create a new dealership:
- ✅ Database record is created with a unique ID
- ✅ Dealership appears in all admin dropdown selectors
- ✅ Default values are set for theme, colors, fonts, etc.
- ✅ A blank website is ready to be configured

## Configuring a New Dealership

After creating the dealership, you'll need to:

1. **Create an Owner Account** (User Management)
   - User Type: Dealership Owner
   - Assign to the new dealership
   - Provide credentials to the client

2. **Configure Basic Settings** (Dealership Settings)
   - Upload logo
   - Set theme colors
   - Configure hero image/video
   - Add finance and warranty policies
   - Set up navigation menu
   - Add social media links

3. **Add Content** (Various Admin Pages)
   - Add vehicles to inventory
   - Create blog posts
   - Customize promotional panels

## Permission Levels

- **System Administrator** (you)
  - Create/view all dealerships
  - Manage any dealership's content
  - Create dealership owner accounts

- **Dealership Owner** (client)
  - Full access to their dealership
  - Cannot create new dealerships
  - Can manage their own staff accounts

- **Dealership Staff** (client's employees)
  - Limited access based on permissions
  - Can view but may not edit certain sections

## Troubleshooting

### Can't see "Dealership Management" link?
- Ensure you're logged in as admin
- Link only appears for `user_type: 'admin'`
- Check browser console for errors

### Form validation errors?
- Ensure all required fields are filled
- Email must be valid format (contains @ and domain)
- Phone should be formatted properly

### "Access Denied" message?
- Only admins can access this page
- If you're admin and seeing this, clear cookies and log in again

### Dealership created but not showing?
- Refresh the page
- Check browser console for errors
- Verify in database: `SELECT * FROM dealership;`

## API Endpoint

If you need to create dealerships programmatically:

```bash
# Login first to get session cookie
curl -X POST http://localhost:3001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  -c cookies.txt

# Create dealership
curl -X POST http://localhost:3001/api/dealers \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "New Auto Sales",
    "address": "123 Main St, City, ST 12345",
    "phone": "(555) 123-4567",
    "email": "info@newautosales.com"
  }'
```

## Testing the Feature

Run the automated test script:
```bash
node test_dealership_creation.js
```

This will:
1. Login as admin
2. Create a test dealership
3. Verify it was created
4. Display next steps

## Questions or Issues?

- Check `DEALERSHIP_MANAGEMENT_FEATURE.md` for detailed documentation
- Review backend logs for API errors
- Check browser console for frontend errors
- Ensure database connection is working
