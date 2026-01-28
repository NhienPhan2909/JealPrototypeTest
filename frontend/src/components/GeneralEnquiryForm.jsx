/**
 * @fileoverview General enquiry form component for homepage.
 * Allows customers to submit general enquiries with name, email, phone, and message.
 * Submissions are stored in the database and viewable in the admin Lead Inbox.
 * 
 * SECURITY: Includes reCAPTCHA verification to prevent spam and automated submissions.
 */

import { useState, useRef } from 'react';
import { useDealershipContext } from '../context/DealershipContext';
import ReCaptcha from './ReCaptcha';
import apiRequest from '../utils/api';

/**
 * GeneralEnquiryForm - Homepage general enquiry form.
 *
 * @component
 *
 * Provides a form for customers to submit general enquiries that aren't related
 * to specific vehicles. Form data is submitted to /api/leads endpoint without
 * vehicle_id, and appears in admin Lead Inbox as "General Enquiry".
 *
 * SECURITY: Requires CAPTCHA verification before submission to prevent spam.
 *
 * Validation Rules:
 * - All fields are required
 * - Email must be valid format
 * - Phone number validation
 * - Message max length: 5000 characters
 * - CAPTCHA verification required
 *
 * @example
 * <GeneralEnquiryForm />
 */
function GeneralEnquiryForm() {
  const { currentDealershipId } = useDealershipContext();
  const captchaRef = useRef();

  // Form state
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    message: ''
  });

  // UI state
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitSuccess, setSubmitSuccess] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const [captchaToken, setCaptchaToken] = useState(null);

  /**
   * Validates email format using basic regex
   * @param {string} email - Email address to validate
   * @returns {boolean} True if valid, false otherwise
   */
  const validateEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  /**
   * Validates phone number (basic validation)
   * @param {string} phone - Phone number to validate
   * @returns {boolean} True if valid, false otherwise
   */
  const validatePhone = (phone) => {
    // Basic validation: at least 10 digits
    const digitsOnly = phone.replace(/\D/g, '');
    return digitsOnly.length >= 10;
  };

  /**
   * Validates all form fields
   * @returns {Object} Object with field names as keys and error messages as values
   */
  const validateForm = () => {
    const newErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    }

    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!validateEmail(formData.email)) {
      newErrors.email = 'Invalid email format';
    }

    if (!formData.phone.trim()) {
      newErrors.phone = 'Phone is required';
    } else if (!validatePhone(formData.phone)) {
      newErrors.phone = 'Invalid phone number';
    }

    if (!formData.message.trim()) {
      newErrors.message = 'Message is required';
    } else if (formData.message.length > 5000) {
      newErrors.message = 'Message must be 5000 characters or less';
    }

    if (!captchaToken) {
      newErrors.captcha = 'Please complete the CAPTCHA verification';
    }

    return newErrors;
  };

  /**
   * Handles CAPTCHA verification
   * @param {string} token - CAPTCHA token from Google
   */
  const handleCaptchaChange = (token) => {
    setCaptchaToken(token);
    // Clear CAPTCHA error if it exists
    if (errors.captcha) {
      setErrors(prev => ({
        ...prev,
        captcha: ''
      }));
    }
  };

  /**
   * Handles input changes
   * @param {Event} e - Input change event
   */
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear error for this field when user types
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  /**
   * Handles form submission
   * @param {Event} e - Form submit event
   */
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Reset status messages
    setSubmitSuccess(false);
    setSubmitError('');

    // Validate form
    const newErrors = validateForm();
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await fetch('/api/leads', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          dealershipId: currentDealershipId,
          name: formData.name,
          email: formData.email,
          phone: formData.phone,
          message: formData.message,
          captchaToken: captchaToken
        })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Failed to submit enquiry');
      }

      // Success - reset form and show success message
      setSubmitSuccess(true);
      setFormData({
        name: '',
        email: '',
        phone: '',
        message: ''
      });
      setErrors({});
      setCaptchaToken(null);
      if (captchaRef.current) {
        captchaRef.current.reset();
      }

      // Auto-hide success message after 5 seconds
      setTimeout(() => {
        setSubmitSuccess(false);
      }, 5000);
    } catch (error) {
      console.error('Error submitting enquiry:', error);
      setSubmitError(error.message || 'Failed to submit enquiry. Please try again.');
      // Reset CAPTCHA on error
      setCaptchaToken(null);
      if (captchaRef.current) {
        captchaRef.current.reset();
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6 h-full">
      <h2 className="text-2xl font-bold mb-4 text-gray-900">General Enquiry</h2>
      <p className="text-gray-600 mb-6">Have a question? Send us a message and we'll get back to you soon.</p>

      {/* Success Message */}
      {submitSuccess && (
        <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
          Thank you for your enquiry! We'll be in touch soon.
        </div>
      )}

      {/* Error Message */}
      {submitError && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {submitError}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Name Field */}
        <div className="mb-4">
          <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
            Name <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            id="name"
            name="name"
            value={formData.name}
            onChange={handleChange}
            className={`input-field ${errors.name ? 'border-red-500' : ''}`}
            placeholder="Your full name"
          />
          {errors.name && (
            <p className="text-red-500 text-sm mt-1">{errors.name}</p>
          )}
        </div>

        {/* Email Field */}
        <div className="mb-4">
          <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
            Email <span className="text-red-500">*</span>
          </label>
          <input
            type="email"
            id="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            className={`input-field ${errors.email ? 'border-red-500' : ''}`}
            placeholder="your.email@example.com"
          />
          {errors.email && (
            <p className="text-red-500 text-sm mt-1">{errors.email}</p>
          )}
        </div>

        {/* Phone Field */}
        <div className="mb-4">
          <label htmlFor="phone" className="block text-sm font-medium text-gray-700 mb-1">
            Phone <span className="text-red-500">*</span>
          </label>
          <input
            type="tel"
            id="phone"
            name="phone"
            value={formData.phone}
            onChange={handleChange}
            className={`input-field ${errors.phone ? 'border-red-500' : ''}`}
            placeholder="(123) 456-7890"
          />
          {errors.phone && (
            <p className="text-red-500 text-sm mt-1">{errors.phone}</p>
          )}
        </div>

        {/* Message Field */}
        <div className="mb-6">
          <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-1">
            Message <span className="text-red-500">*</span>
          </label>
          <textarea
            id="message"
            name="message"
            value={formData.message}
            onChange={handleChange}
            rows="4"
            className={`input-field ${errors.message ? 'border-red-500' : ''}`}
            placeholder="Tell us what you're looking for or ask any questions..."
          />
          {errors.message && (
            <p className="text-red-500 text-sm mt-1">{errors.message}</p>
          )}
          <p className="text-gray-500 text-xs mt-1">
            {formData.message.length} / 5000 characters
          </p>
        </div>

        {/* CAPTCHA */}
        <div className="mb-6">
          <ReCaptcha ref={captchaRef} onChange={handleCaptchaChange} />
          {errors.captcha && (
            <p className="text-red-500 text-sm mt-1 text-center">{errors.captcha}</p>
          )}
        </div>

        {/* Submit Button */}
        <div className="flex justify-center">
          <button
            type="submit"
            disabled={isSubmitting}
            className={`px-8 py-3 text-lg rounded font-semibold transition ${
              isSubmitting ? 'opacity-50 cursor-not-allowed' : 'hover:opacity-90'
            }`}
            style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
          >
            {isSubmitting ? 'Sending...' : 'Submit Enquiry'}
          </button>
        </div>
      </form>
    </div>
  );
}

export default GeneralEnquiryForm;
