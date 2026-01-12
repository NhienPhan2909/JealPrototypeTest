# General Enquiry Form - Implementation Documentation

## Overview
A general enquiry form has been added to the homepage, positioned side-by-side with the "Find your perfect vehicles" search widget.

## Features

### Form Fields
- **Name** (required) - Customer's full name
- **Email** (required) - Validated email format
- **Phone** (required) - Minimum 10 digits validation
- **Message** (required) - Up to 5000 characters with counter

### Validation
- Client-side validation with inline error messages
- Email format validation (user@example.com)
- Phone number validation (minimum 10 digits)
- Character counter for message field
- All fields are required

### User Experience
- Real-time error clearing as user types
- Success message after submission (auto-hides after 5 seconds)
- Error message if submission fails
- Form resets after successful submission
- Submit button disabled during submission
- Loading state ("Sending..." text)

## Layout

### Desktop (≥1024px)
```
┌─────────────────────────────────────────────────────┐
│                    Hero Section                      │
└─────────────────────────────────────────────────────┘
┌────────────────────────┬────────────────────────────┐
│   Find Your Perfect    │   General Enquiry Form     │
│   Vehicle Widget       │                            │
│   (Search filters)     │   - Name field             │
│                        │   - Email field            │
│                        │   - Phone field            │
│                        │   - Message textarea       │
│                        │   - Submit button          │
└────────────────────────┴────────────────────────────┘
```

### Mobile (<1024px)
```
┌─────────────────────────┐
│    Hero Section         │
└─────────────────────────┘
┌─────────────────────────┐
│ Find Your Perfect       │
│ Vehicle Widget          │
└─────────────────────────┘
┌─────────────────────────┐
│ General Enquiry Form    │
│                         │
│ (Full width, stacked)   │
└─────────────────────────┘
```

## Backend Integration

### API Endpoint
- **POST** `/api/leads`
- **Request Body:**
  ```json
  {
    "dealership_id": 1,
    "name": "John Doe",
    "email": "john.doe@example.com",
    "phone": "(555) 123-4567",
    "message": "Customer message here"
  }
  ```
- **Note:** `vehicle_id` is intentionally omitted for general enquiries

### Security
- All inputs are sanitized by the backend (XSS prevention)
- HTML entities are escaped before storage
- Field length validation enforced
- Email format validation

## Admin Lead Inbox

### Display
- General enquiries appear with other leads in the Lead Inbox
- "Vehicle" column shows **"General Enquiry"** in gray text when `vehicle_id` is null
- Vehicle-specific enquiries show a clickable link to the vehicle
- All standard lead fields are displayed: name, email, phone, message, date
- Same filtering and sorting as vehicle enquiries

### Example Row
```
| Name     | Email          | Phone        | Vehicle          | Message    | Date       | Actions    |
|----------|----------------|--------------|------------------|------------|------------|------------|
| John Doe | john@email.com | 555-123-4567 | General Enquiry  | Hi, I am...| 12/08/2025 | Call Email |
```

## Files Modified

### Created
- `frontend/src/components/GeneralEnquiryForm.jsx` - New form component

### Updated
- `frontend/src/pages/public/Home.jsx` - Added side-by-side layout
- `frontend/src/components/SearchWidget.jsx` - Adjusted styling for grid layout
- `frontend/src/components/GeneralEnquiryForm.jsx` - Adjusted styling for grid layout

### Existing (No Changes Needed)
- `backend/routes/leads.js` - Already supports general enquiries (vehicle_id optional)
- `frontend/src/pages/admin/LeadInbox.jsx` - Already displays "General Enquiry" label

## Testing

### Manual Testing Steps
1. Visit homepage at `http://localhost:3000`
2. Scroll to the section with both widgets
3. Fill out the General Enquiry form:
   - Enter your name
   - Enter a valid email
   - Enter a phone number (10+ digits)
   - Write a message
4. Click "Submit Enquiry"
5. Verify success message appears
6. Login to admin panel
7. Navigate to Lead Inbox
8. Verify the general enquiry appears with "General Enquiry" label

### Automated Test
Run `node test_general_enquiry.js` to verify API submission works correctly.

## Browser Compatibility
- Responsive design works on all modern browsers
- Grid layout with fallback to stacked layout on mobile
- Tailwind CSS handles browser prefixes automatically

## Future Enhancements (Optional)
- Add file attachment capability
- Add preferred contact method radio buttons
- Add subject/category dropdown
- Add CAPTCHA for spam prevention
- Add email notification to dealership when form is submitted
- Add auto-reply email to customer
