# Email Notification Setup Guide

This guide explains how to configure email notifications for new leads in the Jeal Prototype system.

## Overview

When a customer submits a new lead (enquiry) through the website, the system automatically sends an email notification to the dealership's email address. The notification includes:

- Customer name
- Customer email
- Customer phone
- Vehicle information (if the lead is for a specific vehicle)
- Customer message

## Configuration Steps

### 1. Email Provider Setup

You need to configure an SMTP email service. Here are common options:

#### Option A: Gmail

1. Enable 2-factor authentication on your Gmail account
2. Generate an App-Specific Password:
   - Go to https://myaccount.google.com/apppasswords
   - Select "Mail" and your device
   - Copy the generated 16-character password

3. Update `.env` file:
   ```env
   EMAIL_HOST=smtp.gmail.com
   EMAIL_PORT=587
   EMAIL_USER=your-email@gmail.com
   EMAIL_PASSWORD=your-16-char-app-password
   EMAIL_FROM=your-email@gmail.com
   ```

#### Option B: SendGrid

1. Sign up for SendGrid (free tier available)
2. Create an API key from the SendGrid dashboard
3. Update `.env` file:
   ```env
   EMAIL_HOST=smtp.sendgrid.net
   EMAIL_PORT=587
   EMAIL_USER=apikey
   EMAIL_PASSWORD=YOUR_SENDGRID_API_KEY
   EMAIL_FROM=noreply@yourdomain.com
   ```

#### Option C: Other SMTP Providers

For other providers (Mailgun, AWS SES, etc.), update the `.env` file with their SMTP settings:
```env
EMAIL_HOST=smtp.yourprovider.com
EMAIL_PORT=587
EMAIL_USER=your-username
EMAIL_PASSWORD=your-password
EMAIL_FROM=noreply@yourdomain.com
```

### 2. Environment Variables

The following environment variables must be configured in your `.env` file:

| Variable | Description | Example |
|----------|-------------|---------|
| `EMAIL_HOST` | SMTP server hostname | `smtp.gmail.com` |
| `EMAIL_PORT` | SMTP server port (587 for TLS, 465 for SSL) | `587` |
| `EMAIL_USER` | SMTP authentication username | `your-email@gmail.com` |
| `EMAIL_PASSWORD` | SMTP authentication password | `your-app-password` |
| `EMAIL_FROM` | Sender email address shown in notifications | `noreply@yourdomain.com` |

### 3. Dealership Email Configuration

Each dealership must have an email address configured in the database:

1. Go to the Admin CMS
2. Navigate to Settings
3. Ensure the dealership email field is filled with a valid email address
4. This is the email address that will receive lead notifications

## Testing

### Manual Test

You can test the email notification by submitting a test lead:

1. Start your backend server: `cd backend && npm run dev`
2. Submit a test lead through your frontend or using a tool like Postman:

```bash
curl -X POST http://localhost:5000/api/leads \
  -H "Content-Type: application/json" \
  -d '{
    "dealership_id": 1,
    "name": "Test Customer",
    "email": "customer@example.com",
    "phone": "(555) 123-4567",
    "message": "This is a test enquiry"
  }'
```

3. Check the server logs for:
   - "Lead notification email sent to: [dealership-email]"
   - Or warning messages if email configuration is not set

4. Check the dealership's email inbox for the notification

### Development Mode

If email configuration is not set (common in development), the system will:
- Log a warning: "Email configuration not set. Skipping email notification."
- Print the lead data to the console
- Continue to create the lead successfully (email failure won't block lead creation)

## Email Template

The notification email includes:

**Subject**: `New Lead: [Customer Name] - [Vehicle or General Enquiry]`

**Content**:
- Branded header with "New Lead Received"
- Customer details in a formatted table
- Professional styling with your brand colors
- Call to action to respond promptly

## Troubleshooting

### Email not being sent

1. **Check environment variables**: Ensure all EMAIL_* variables are set in `.env`
2. **Check dealership email**: Verify the dealership has a valid email in the database
3. **Check server logs**: Look for error messages in the console
4. **Test SMTP credentials**: Verify your SMTP credentials are correct
5. **Check firewall**: Ensure port 587 (or 465) is not blocked

### Gmail "Less secure app" error

- Gmail no longer supports "less secure apps"
- You MUST use an App-Specific Password (see Gmail setup above)
- Do NOT use your regular Gmail password

### SendGrid not working

- Verify your API key is active
- Ensure you've completed SendGrid's sender verification
- Check SendGrid dashboard for delivery logs

### Email goes to spam

- Configure SPF and DKIM records for your domain
- Use a verified sender email address
- Consider using a professional email service like SendGrid
- Avoid trigger words in the subject line

## Security Notes

- **Never commit credentials**: The `.env` file is in `.gitignore`
- **Use app-specific passwords**: Don't use your primary email password
- **Rotate credentials**: Change passwords if they're compromised
- **Use environment variables**: Never hardcode credentials in source code

## Production Deployment

For production:

1. Use a professional email service (SendGrid, AWS SES, Mailgun)
2. Configure custom domain email addresses
3. Set up email monitoring and logging
4. Configure retry logic for failed emails
5. Set up email rate limiting to avoid spam complaints

## Support

If you encounter issues:

1. Check the server console logs
2. Verify all environment variables are set correctly
3. Test your SMTP credentials with a simple nodemailer test script
4. Consult your email provider's documentation
