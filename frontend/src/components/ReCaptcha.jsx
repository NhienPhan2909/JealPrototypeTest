/**
 * @fileoverview ReCaptcha wrapper component.
 * Provides Google reCAPTCHA v2 integration for form spam protection.
 */

import { useRef, useImperativeHandle, forwardRef } from 'react';
import ReCAPTCHA from 'react-google-recaptcha';

/**
 * ReCaptcha - Google reCAPTCHA v2 component wrapper.
 *
 * @component
 * @param {Object} props
 * @param {Function} props.onChange - Callback when CAPTCHA is completed (receives token)
 * @param {Object} ref - Forwarded ref to access reset() method
 *
 * Usage:
 * const captchaRef = useRef();
 * <ReCaptcha ref={captchaRef} onChange={handleCaptchaChange} />
 * 
 * To reset: captchaRef.current.reset()
 *
 * Environment Variables Required:
 * - VITE_RECAPTCHA_SITE_KEY: Your reCAPTCHA site key from Google
 *
 * @example
 * const captchaRef = useRef();
 * const [captchaToken, setCaptchaToken] = useState(null);
 * 
 * <ReCaptcha 
 *   ref={captchaRef} 
 *   onChange={(token) => setCaptchaToken(token)} 
 * />
 */
const ReCaptcha = forwardRef(({ onChange }, ref) => {
  const recaptchaRef = useRef();

  // Expose reset method to parent component
  useImperativeHandle(ref, () => ({
    reset: () => {
      if (recaptchaRef.current) {
        recaptchaRef.current.reset();
      }
    }
  }));

  const siteKey = import.meta.env.VITE_RECAPTCHA_SITE_KEY;

  // If site key is not configured, show warning message
  if (!siteKey) {
    return (
      <div className="bg-yellow-100 border border-yellow-400 text-yellow-800 px-4 py-2 rounded text-sm">
        ⚠️ CAPTCHA not configured. Set VITE_RECAPTCHA_SITE_KEY environment variable.
      </div>
    );
  }

  return (
    <div className="flex justify-center my-4">
      <ReCAPTCHA
        ref={recaptchaRef}
        sitekey={siteKey}
        onChange={onChange}
      />
    </div>
  );
});

ReCaptcha.displayName = 'ReCaptcha';

export default ReCaptcha;
