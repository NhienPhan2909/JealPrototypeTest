import { useState, useEffect, useRef } from 'react';
import ReCaptcha from './ReCaptcha';
import apiRequest from '../utils/api';

/**
 * EnquiryForm - Customer enquiry form for vehicle detail page.
 *
 * @component
 * @param {Object} props
 * @param {number} props.vehicleId - Vehicle ID for the enquiry
 * @param {number} props.dealershipId - Dealership ID receiving the enquiry
 * @param {string} props.vehicleTitle - Vehicle title for auto-populating message
 * @param {string} props.dealershipPhone - Dealership phone for error message
 *
 * Features:
 * - Client-side validation (required fields, email format)
 * - CAPTCHA verification to prevent spam
 * - Auto-populates message with vehicle title
 * - Loading state during submission (disabled button)
 * - Success state with confirmation message
 * - Error state with dealership phone fallback
 * - Form reset after successful submission
 *
 * @example
 * <EnquiryForm
 *   vehicleId={1}
 *   dealershipId={1}
 *   vehicleTitle="2015 Toyota Camry SE"
 *   dealershipPhone="(555) 123-4567"
 * />
 */
function EnquiryForm({ vehicleId, dealershipId, vehicleTitle, dealershipPhone }) {
  const captchaRef = useRef();
  
  // Form field state
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [message, setMessage] = useState(`I'm interested in the ${vehicleTitle}.`);

  // UI state
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState(null);
  const [captchaToken, setCaptchaToken] = useState(null);

  // Email validation regex
  const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  /**
   * Updates message field when vehicleTitle prop changes.
   * Ensures message stays in sync if user navigates between vehicles.
   */
  useEffect(() => {
    // Only update message if it's still the default template or empty
    // Don't override if user has customized their message
    if (!message || message === `I'm interested in the ${vehicleTitle}.` ||
        message.startsWith("I'm interested in the")) {
      setMessage(`I'm interested in the ${vehicleTitle}.`);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehicleTitle]); // Intentionally omit 'message' to prevent infinite loop

  /**
   * Validates form fields and returns errors object.
   * @returns {Object} Errors object with field-specific error messages
   */
  function validateForm() {
    const newErrors = {};

    if (!name.trim()) {
      newErrors.name = 'Name is required';
    }
    if (!email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!EMAIL_REGEX.test(email)) {
      newErrors.email = 'Valid email is required';
    }
    if (!phone.trim()) {
      newErrors.phone = 'Phone is required';
    }
    if (!message.trim()) {
      newErrors.message = 'Message is required';
    }
    if (!captchaToken) {
      newErrors.captcha = 'Please complete the CAPTCHA verification';
    }

    return newErrors;
  }

  /**
   * Handles CAPTCHA verification
   */
  function handleCaptchaChange(token) {
    setCaptchaToken(token);
    if (errors.captcha) {
      setErrors(prev => ({ ...prev, captcha: '' }));
    }
  }

  /**
   * Handles form submission.
   * Validates form, calls API, handles success/error states.
   */
  async function handleSubmit(e) {
    e.preventDefault();

    // Reset previous error
    setError(null);

    // Validate form
    const newErrors = validateForm();
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    // Clear errors
    setErrors({});

    // Set loading state
    setSubmitting(true);

    try {
      const response = await fetch('/api/leads', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          dealershipId: dealershipId,
          vehicleId: vehicleId,
          name,
          email,
          phone,
          message,
          captchaToken
        })
      });

      if (response.status === 201) {
        // Success - show success message
        setSubmitted(true);
        // Clear form fields
        setName('');
        setEmail('');
        setPhone('');
        setMessage(`I'm interested in the ${vehicleTitle}.`);
        setCaptchaToken(null);
        if (captchaRef.current) {
          captchaRef.current.reset();
        }
      } else {
        // API returned error - safely parse JSON response
        try {
          const data = await response.json();
          setError(data.error || 'Unable to submit enquiry. Please try again.');
        } catch (jsonError) {
          // Response wasn't JSON (e.g., 500 plain text error)
          console.error('Failed to parse error response:', jsonError);
          setError('Unable to submit enquiry. Please try again.');
        }
        // Reset CAPTCHA on error
        setCaptchaToken(null);
        if (captchaRef.current) {
          captchaRef.current.reset();
        }
      }
    } catch (err) {
      console.error('Failed to submit enquiry:', err);
      setError(`Unable to submit enquiry. Please try again or call us at ${dealershipPhone}.`);
      // Reset CAPTCHA on error
      setCaptchaToken(null);
      if (captchaRef.current) {
        captchaRef.current.reset();
      }
    } finally {
      setSubmitting(false);
    }
  }

  /**
   * Resets form to initial state for another submission.
   */
  function resetForm() {
    setSubmitted(false);
    setError(null);
    setErrors({});
    setName('');
    setEmail('');
    setPhone('');
    setMessage(`I'm interested in the ${vehicleTitle}.`);
    setCaptchaToken(null);
    if (captchaRef.current) {
      captchaRef.current.reset();
    }
  }

  // Success state - show confirmation message
  if (submitted) {
    return (
      <div className="card mt-6">
        <div className="bg-green-50 border border-green-500 text-green-800 p-4 rounded mb-4" role="alert">
          <h3 className="text-xl font-semibold mb-2">Thank you! We'll contact you soon.</h3>
          <p>We've received your enquiry and will be in touch shortly.</p>
        </div>
        <div className="flex gap-4">
          <button onClick={resetForm} className="btn-primary">
            Submit Another Enquiry
          </button>
          <a href="/inventory" className="btn-secondary">
            Back to Inventory
          </a>
        </div>
      </div>
    );
  }

  // Form state - show enquiry form
  return (
    <div className="card mt-6">
      <h2 className="text-2xl font-semibold mb-4">Interested in this vehicle? Contact us!</h2>

      {/* Error message (API error, not field validation) */}
      {error && (
        <div className="bg-red-50 border border-red-500 text-red-800 p-4 rounded mb-4" role="alert">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Name field */}
        <div className="mb-4">
          <label htmlFor="name" className="block text-sm font-medium mb-1">Name *</label>
          <input
            type="text"
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="input-field"
            disabled={submitting}
          />
          {errors.name && <p className="text-red-600 text-sm mt-1" role="alert">{errors.name}</p>}
        </div>

        {/* Email field */}
        <div className="mb-4">
          <label htmlFor="email" className="block text-sm font-medium mb-1">Email *</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="input-field"
            disabled={submitting}
          />
          {errors.email && <p className="text-red-600 text-sm mt-1" role="alert">{errors.email}</p>}
        </div>

        {/* Phone field */}
        <div className="mb-4">
          <label htmlFor="phone" className="block text-sm font-medium mb-1">Phone *</label>
          <input
            type="tel"
            id="phone"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            className="input-field"
            disabled={submitting}
          />
          {errors.phone && <p className="text-red-600 text-sm mt-1" role="alert">{errors.phone}</p>}
        </div>

        {/* Message field */}
        <div className="mb-4">
          <label htmlFor="message" className="block text-sm font-medium mb-1">Message *</label>
          <textarea
            id="message"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            rows="4"
            className="input-field"
            disabled={submitting}
          />
          {errors.message && <p className="text-red-600 text-sm mt-1" role="alert">{errors.message}</p>}
        </div>

        {/* CAPTCHA */}
        <div className="mb-4">
          <ReCaptcha ref={captchaRef} onChange={handleCaptchaChange} />
          {errors.captcha && <p className="text-red-600 text-sm mt-1 text-center" role="alert">{errors.captcha}</p>}
        </div>

        {/* Submit button */}
        <button
          type="submit"
          disabled={submitting}
          aria-busy={submitting}
          className={submitting ? 'px-4 py-3 rounded cursor-not-allowed text-white' : 'px-4 py-3 rounded font-semibold hover:opacity-90 transition'}
          style={submitting ? { backgroundColor: '#9CA3AF' } : { backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
        >
          {submitting ? 'Submitting...' : 'Submit Enquiry'}
        </button>
      </form>
    </div>
  );
}

export default EnquiryForm;
