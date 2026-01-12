# CAPTCHA Security Feature

## Overview
All public-facing forms on the dealership website now require Google reCAPTCHA v2 verification to prevent spam and automated submissions.

## Forms Protected
1. **General Enquiry Form** (Homepage)
2. **Vehicle Enquiry Form** (Vehicle Detail Page)
3. **Sell Your Car Form** (Sell Your Car Page)

## Setup Instructions

### 1. Get Google reCAPTCHA Keys

1. Visit [Google reCAPTCHA Admin Console](https://www.google.com/recaptcha/admin)
2. Register a new site with reCAPTCHA v2 "I'm not a robot" Checkbox
3. Add your domain(s) to the allowed domains list
4. You will receive:
   - **Site Key** (public, used in frontend)
   - **Secret Key** (private, used in backend)

### 2. Configure Environment Variables

#### Backend Configuration
Add to `backend/.env` (or your environment configuration):

```env
RECAPTCHA_SECRET_KEY=your_secret_key_here
```

#### Frontend Configuration
Add to `frontend/.env` or `frontend/.env.local`:

```env
VITE_RECAPTCHA_SITE_KEY=your_site_key_here
```

### 3. Development vs Production

For **development/testing**, you can use Google's test keys:
- **Site Key**: `6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI`
- **Secret Key**: `6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe`

⚠️ **Test keys always pass validation** - they should NEVER be used in production!

For **production**, use real keys obtained from Google reCAPTCHA Admin Console.

## How It Works

### Frontend
1. User fills out the form
2. User completes the CAPTCHA challenge (checks "I'm not a robot")
3. Form validates that CAPTCHA is completed before submission
4. CAPTCHA token is included in the form submission payload

### Backend
1. Middleware intercepts POST requests to `/api/leads` and `/api/sales-requests`
2. Extracts `captchaToken` from request body
3. Verifies token with Google's reCAPTCHA API
4. If verification succeeds, request proceeds
5. If verification fails, returns 400 error

## Security Features

- **Spam Prevention**: Prevents automated bots from submitting forms
- **Rate Limiting**: Google reCAPTCHA has built-in rate limiting
- **Token Expiration**: CAPTCHA tokens expire after ~2 minutes
- **Single-Use**: Each token can only be verified once
- **Graceful Degradation**: If CAPTCHA keys are not configured, backend logs warning but allows requests (dev only)

## User Experience

### Success Flow
1. User fills form → Completes CAPTCHA → Submits
2. Form resets, CAPTCHA resets
3. Success message shown

### Error Flow
1. User fills form → Submits without CAPTCHA
2. Validation error: "Please complete the CAPTCHA verification"
3. User completes CAPTCHA → Submits again → Success

### Network Error Flow
1. User fills form → Completes CAPTCHA → Submits
2. Network error or CAPTCHA verification fails
3. CAPTCHA is reset (user must complete again)
4. Error message shown

## Testing

### Manual Testing
1. **Test without CAPTCHA**: Try submitting form without completing CAPTCHA
   - ✅ Should show validation error
2. **Test with CAPTCHA**: Complete CAPTCHA and submit
   - ✅ Should submit successfully
3. **Test CAPTCHA reset**: Submit with error, verify CAPTCHA resets
   - ✅ CAPTCHA should be unchecked after error

### Automated Testing
When running automated tests, you can:
1. Use test keys (always pass)
2. Mock the CAPTCHA verification in tests
3. Skip CAPTCHA by not configuring `RECAPTCHA_SECRET_KEY` (dev only)

## Troubleshooting

### CAPTCHA Not Showing
- Check `VITE_RECAPTCHA_SITE_KEY` is set in frontend environment
- Check browser console for errors
- Verify domain is whitelisted in reCAPTCHA Admin Console

### Verification Always Failing
- Check `RECAPTCHA_SECRET_KEY` is correct in backend environment
- Verify secret key matches the site key
- Check backend logs for detailed error messages
- Ensure backend can reach `https://www.google.com/recaptcha/api/siteverify`

### "CAPTCHA not configured" Warning
- Yellow warning box appears if `VITE_RECAPTCHA_SITE_KEY` is not set
- Configure the environment variable and restart dev server

## Files Modified

### Backend
- `backend/middleware/captchaVerification.js` - CAPTCHA verification middleware
- `backend/routes/leads.js` - Added CAPTCHA middleware to POST route
- `backend/routes/salesRequests.js` - Added CAPTCHA middleware to POST route

### Frontend
- `frontend/src/components/ReCaptcha.jsx` - Reusable CAPTCHA component
- `frontend/src/components/GeneralEnquiryForm.jsx` - Added CAPTCHA
- `frontend/src/components/EnquiryForm.jsx` - Added CAPTCHA
- `frontend/src/pages/public/SellYourCar.jsx` - Added CAPTCHA

## Dependencies Added

### Backend
```json
{
  "node-fetch": "^2.x"
}
```

### Frontend
```json
{
  "react-google-recaptcha": "^3.x"
}
```

## API Changes

All form submission endpoints now accept an additional field:

```json
{
  "captchaToken": "string (required)",
  // ... other form fields
}
```

Error responses:
```json
{
  "error": "CAPTCHA verification required. Please complete the CAPTCHA."
}
```
```json
{
  "error": "CAPTCHA verification failed. Please try again."
}
```

## Privacy Considerations

Google reCAPTCHA collects:
- User IP address
- Browser information
- User interactions with the CAPTCHA

Ensure your privacy policy is updated to reflect the use of Google reCAPTCHA.

## Additional Resources

- [Google reCAPTCHA Documentation](https://developers.google.com/recaptcha/docs/display)
- [reCAPTCHA Admin Console](https://www.google.com/recaptcha/admin)
- [react-google-recaptcha GitHub](https://github.com/dozoisch/react-google-recaptcha)
