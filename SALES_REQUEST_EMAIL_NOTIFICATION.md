# Sales Request Email Notification Feature

## Overview
Added email notification functionality that automatically sends an email to the dealership when a customer submits a new "Sell Your Car" request through the website.

## Implementation Date
December 31, 2024

## Changes Made

### 1. Email Service (`backend/services/emailService.js`)
- **Added Function**: `sendNewSalesRequestNotification(dealershipEmail, salesRequestData)`
- **Purpose**: Sends a formatted email notification to the dealership when a new sales request is received
- **Email Content Includes**:
  - Customer Information (name, email, phone)
  - Vehicle Details (make, model, year, kilometers)
  - Additional message from customer (if provided)
- **Features**:
  - HTML and plain text versions for email client compatibility
  - Professional styling matching the existing lead notification emails
  - Graceful handling if email configuration is not set (dev/test environments)

### 2. Sales Request Routes (`backend/routes/salesRequests.js`)
- **Modified Endpoint**: `POST /api/sales-requests`
- **Added Dependencies**:
  - `dealersDb` - to fetch dealership email address
  - `sendNewSalesRequestNotification` - to send email notification
- **Workflow**:
  1. Validate and sanitize customer input
  2. Create sales request in database
  3. Fetch dealership email from database
  4. Send email notification to dealership
  5. Log success/failure (email errors don't fail the request)
- **Error Handling**: Email sending errors are logged but don't prevent successful sales request creation

## Email Template

### Subject Line
```
New Sales Request: [Year] [Make] [Model]
Example: New Sales Request: 2018 Toyota Camry
```

### Content Sections
1. **Header**: "🚗💰 New 'Sell Your Car' Request"
2. **Customer Information Section**:
   - Name
   - Email
   - Phone
3. **Vehicle Details Section**:
   - Make
   - Model
   - Year
   - Kilometers (formatted with thousands separator)
4. **Additional Information Section**:
   - Customer's additional message (or "None" if not provided)
5. **Footer**: Reminder to contact customer for valuation discussion

## Configuration Requirements

Email notifications require the following environment variables in `.env`:

```env
EMAIL_HOST=smtp.gmail.com          # SMTP server host
EMAIL_PORT=587                     # SMTP port (587 for TLS, 465 for SSL)
EMAIL_USER=your-email@gmail.com    # Email account username
EMAIL_PASSWORD=your-app-password   # Email account password or app-specific password
EMAIL_FROM=noreply@yourdomain.com  # Sender email address
```

See `EMAIL_SETUP.md` for detailed email configuration instructions.

## Testing

### Manual Testing Steps

1. **Start the backend server**:
   ```bash
   cd backend
   npm start
   ```

2. **Run the test script**:
   ```bash
   node test_sales_request_email.js
   ```

3. **Or test via the frontend**:
   - Navigate to any dealership website
   - Click "Sell Your Car" in the navigation
   - Fill out and submit the form
   - Check the backend console for email sending confirmation
   - Check the dealership email inbox for the notification

### Expected Results

- ✅ Sales request is created in the database
- ✅ Console logs: "Sales request notification email sent to: [email]"
- ✅ Dealership receives email with all form details
- ✅ Email includes properly formatted vehicle and customer information
- ✅ Additional message is displayed (if provided)

### Error Scenarios

**If email configuration is missing**:
- Console logs warning: "Email configuration not set. Skipping email notification."
- Sales request is still created successfully
- No email is sent

**If dealership email is not found**:
- Console logs warning: "Dealership email not found for dealership_id: [id]"
- Sales request is still created successfully
- No email is sent

**If email sending fails**:
- Console logs error with details
- Sales request is still created successfully
- Admin should check email configuration

## Files Modified

1. `backend/services/emailService.js`
   - Added `sendNewSalesRequestNotification()` function
   - Updated module exports

2. `backend/routes/salesRequests.js`
   - Added imports for `dealersDb` and `sendNewSalesRequestNotification`
   - Added email notification logic to POST endpoint
   - Added error handling for email failures

## Files Created

1. `test_sales_request_email.js` - Test script for email notification feature

## Security Considerations

- All input data is sanitized before being included in emails (XSS prevention)
- Email sending errors are caught and logged, preventing sensitive error info from leaking to client
- Email credentials are stored in environment variables, not in code
- Multi-tenant isolation: each dealership only receives notifications for their own sales requests

## Feature Comparison with Lead Notifications

This implementation follows the exact same pattern as the existing lead email notifications:

| Feature | Lead Notifications | Sales Request Notifications |
|---------|-------------------|----------------------------|
| Email Service Function | `sendNewLeadNotification` | `sendNewSalesRequestNotification` |
| Trigger Point | POST /api/leads | POST /api/sales-requests |
| Email Template Style | HTML + Plain text | HTML + Plain text |
| Error Handling | Non-blocking | Non-blocking |
| Configuration | .env variables | .env variables |
| Graceful Degradation | ✅ | ✅ |

## Benefits

1. **Immediate Notification**: Dealerships are notified instantly when customers want to sell their vehicles
2. **Complete Information**: All form fields are included in the email for easy reference
3. **No Login Required**: Staff can review details directly from their email inbox
4. **Professional Presentation**: Well-formatted HTML email with clear sections
5. **Increased Response Rate**: Faster notification leads to quicker customer contact and higher conversion

## Future Enhancements

Potential improvements for consideration:

1. **SMS Notifications**: Send text message alerts in addition to email
2. **Email Templates**: Allow dealerships to customize email template
3. **Auto-Response**: Send confirmation email to customer
4. **Email Tracking**: Track if dealership opened the notification email
5. **Digest Option**: Send daily summary instead of immediate notifications
6. **Attachment Support**: Include vehicle photos if customer uploads them

## Support

For issues or questions:
- Check `EMAIL_SETUP.md` for email configuration help
- Review backend console logs for error messages
- Verify dealership email is set in database
- Test email credentials with test_general_enquiry.js script
