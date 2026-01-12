/**
 * @fileoverview CAPTCHA verification middleware.
 * Validates Google reCAPTCHA tokens to prevent spam and automated form submissions.
 */

const fetch = require('node-fetch');

/**
 * Verifies reCAPTCHA token with Google's API.
 * 
 * @param {string} token - reCAPTCHA token from client
 * @returns {Promise<boolean>} True if verification successful, false otherwise
 */
async function verifyCaptcha(token) {
  const secretKey = process.env.RECAPTCHA_SECRET_KEY;
  
  if (!secretKey) {
    console.error('RECAPTCHA_SECRET_KEY not configured');
    return false;
  }

  if (!token) {
    return false;
  }

  try {
    const response = await fetch('https://www.google.com/recaptcha/api/siteverify', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      },
      body: `secret=${secretKey}&response=${token}`
    });

    const data = await response.json();
    return data.success === true;
  } catch (error) {
    console.error('Error verifying reCAPTCHA:', error);
    return false;
  }
}

/**
 * Express middleware to verify reCAPTCHA token in request body.
 * Expects captchaToken field in request body.
 * 
 * @returns {Function} Express middleware function
 */
function captchaVerification(req, res, next) {
  const { captchaToken } = req.body;

  // Check if CAPTCHA is enabled (allow bypass if not configured)
  if (!process.env.RECAPTCHA_SECRET_KEY) {
    console.warn('CAPTCHA verification skipped - RECAPTCHA_SECRET_KEY not configured');
    return next();
  }

  if (!captchaToken) {
    return res.status(400).json({ 
      error: 'CAPTCHA verification required. Please complete the CAPTCHA.' 
    });
  }

  verifyCaptcha(captchaToken)
    .then(isValid => {
      if (isValid) {
        next();
      } else {
        res.status(400).json({ 
          error: 'CAPTCHA verification failed. Please try again.' 
        });
      }
    })
    .catch(error => {
      console.error('CAPTCHA verification error:', error);
      res.status(500).json({ 
        error: 'CAPTCHA verification error. Please try again.' 
      });
    });
}

module.exports = captchaVerification;
