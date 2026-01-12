# CAPTCHA Quick Start Guide

## 🚀 Quick Setup (5 minutes)

### Step 1: Get reCAPTCHA Keys (2 min)
1. Go to https://www.google.com/recaptcha/admin
2. Click "+" to register a new site
3. Choose **reCAPTCHA v2** → "I'm not a robot" Checkbox
4. Add your domains (e.g., `localhost` for development)
5. Copy your **Site Key** and **Secret Key**

### Step 2: Configure Backend (1 min)
1. Create `backend/.env` file (or add to existing)
2. Add this line:
   ```env
   RECAPTCHA_SECRET_KEY=your_secret_key_here
   ```
3. Restart backend server

### Step 3: Configure Frontend (1 min)
1. Create `frontend/.env.local` file (or add to existing)
2. Add this line:
   ```env
   VITE_RECAPTCHA_SITE_KEY=your_site_key_here
   ```
3. Restart frontend dev server

### Step 4: Test (1 min)
1. Open the website
2. Go to homepage or any vehicle page
3. Fill out the enquiry form
4. You should see the CAPTCHA checkbox
5. Complete CAPTCHA and submit form

## ✅ What's Protected

All customer-facing forms now require CAPTCHA:
- ✅ General Enquiry Form (Homepage)
- ✅ Vehicle Enquiry Form (Vehicle Detail Pages)
- ✅ Sell Your Car Form

## 🧪 Testing with Test Keys

For quick testing without registering, use Google's test keys:

**Frontend** (`frontend/.env.local`):
```env
VITE_RECAPTCHA_SITE_KEY=6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI
```

**Backend** (`backend/.env`):
```env
RECAPTCHA_SECRET_KEY=6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe
```

⚠️ **Warning**: Test keys always pass validation. Never use in production!

## 🔍 Verification

### How to know it's working:
1. **Visual**: You see the "I'm not a robot" checkbox on forms
2. **Without CAPTCHA**: Form shows error "Please complete the CAPTCHA verification"
3. **With CAPTCHA**: Form submits successfully
4. **Backend logs**: Check for "CAPTCHA verification" messages

### What if CAPTCHA doesn't appear?
1. Check frontend environment variable is set
2. Restart dev server (`npm run dev`)
3. Check browser console for errors
4. Verify site key is correct

### What if verification fails?
1. Check backend environment variable is set
2. Restart backend server
3. Verify secret key matches your site key
4. Check backend logs for detailed errors

## 📋 Common Issues

| Issue | Solution |
|-------|----------|
| CAPTCHA not showing | Set `VITE_RECAPTCHA_SITE_KEY` and restart frontend |
| Yellow warning box | Environment variable not set |
| "CAPTCHA verification failed" | Check backend secret key is correct |
| Always fails in production | Ensure domain is whitelisted in reCAPTCHA admin |
| CAPTCHA shows but is small | Check CSS/responsive settings |

## 📚 Full Documentation

See `CAPTCHA_FEATURE.md` for complete documentation including:
- Detailed setup instructions
- Architecture overview
- Security features
- API changes
- Troubleshooting guide
- Privacy considerations

## 🆘 Need Help?

1. Check `CAPTCHA_FEATURE.md` for detailed docs
2. Check browser console for errors
3. Check backend logs for verification errors
4. Verify environment variables are set correctly
5. Ensure dev server is restarted after env changes
