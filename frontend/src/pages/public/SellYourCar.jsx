/**
 * @fileoverview SellYourCar - Public page for customers to submit vehicle sales requests.
 * Includes a form for customers to enter their contact information and vehicle details.
 * 
 * SECURITY: Requires CAPTCHA verification to prevent spam submissions.
 */

import { useState, useRef } from 'react';
import { useDealershipContext } from '../../context/DealershipContext';
import useDealership from '../../hooks/useDealership';
import ReCaptcha from '../../components/ReCaptcha';
import apiRequest from '../../utils/api';

/**
 * SellYourCar - Form for customers to sell their vehicles to the dealership.
 *
 * @component
 * @example
 * <SellYourCar />
 */
function SellYourCar() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership } = useDealership(currentDealershipId);
  const captchaRef = useRef();
  
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    make: '',
    model: '',
    year: '',
    kilometers: '',
    additionalMessage: ''
  });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState(null);
  const [captchaToken, setCaptchaToken] = useState(null);
  const [captchaError, setCaptchaError] = useState('');

  const themeColor = dealership?.theme_color || '#3B82F6';

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleCaptchaChange = (token) => {
    setCaptchaToken(token);
    setCaptchaError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccess(false);
    setCaptchaError('');

    // Validate CAPTCHA
    if (!captchaToken) {
      setCaptchaError('Please complete the CAPTCHA verification');
      setLoading(false);
      return;
    }

    try {
      const response = await fetch('/api/sales-requests', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          dealershipId: currentDealershipId,
          ...formData,
          captchaToken
        })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Failed to submit sales request');
      }

      setSuccess(true);
      setFormData({
        name: '',
        email: '',
        phone: '',
        make: '',
        model: '',
        year: '',
        kilometers: '',
        additionalMessage: ''
      });
      setCaptchaToken(null);
      if (captchaRef.current) {
        captchaRef.current.reset();
      }
    } catch (err) {
      console.error('Error submitting sales request:', err);
      setError(err.message || 'Failed to submit sales request. Please try again.');
      // Reset CAPTCHA on error
      setCaptchaToken(null);
      if (captchaRef.current) {
        captchaRef.current.reset();
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-3xl mx-auto px-4">
        <h1 className="text-4xl font-bold mb-4 text-gray-900">Sell Your Car</h1>
        <p className="text-lg text-gray-600 mb-8">
          Interested in selling your vehicle? Fill out the form below and our team will get back to you with an offer.
        </p>

        {success && (
          <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-6">
            <p className="font-bold">Success!</p>
            <p>Your sales request has been submitted. We'll contact you soon.</p>
          </div>
        )}

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-6">
            <p className="font-bold">Error</p>
            <p>{error}</p>
          </div>
        )}

        <div className="bg-white rounded-lg shadow-md p-8">
          <form onSubmit={handleSubmit}>
            {/* Personal Information */}
            <div className="mb-6">
              <h2 className="text-2xl font-semibold mb-4 text-gray-800">Your Information</h2>
              
              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="name">
                  Name *
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="John Doe"
                />
              </div>

              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="email">
                  Email *
                </label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="john@example.com"
                />
              </div>

              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="phone">
                  Phone *
                </label>
                <input
                  type="tel"
                  id="phone"
                  name="phone"
                  value={formData.phone}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="(555) 123-4567"
                />
              </div>
            </div>

            {/* Vehicle Information */}
            <div className="mb-6">
              <h2 className="text-2xl font-semibold mb-4 text-gray-800">Vehicle Details</h2>
              
              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="make">
                  Make *
                </label>
                <input
                  type="text"
                  id="make"
                  name="make"
                  value={formData.make}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="Toyota"
                />
              </div>

              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="model">
                  Model *
                </label>
                <input
                  type="text"
                  id="model"
                  name="model"
                  value={formData.model}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="Camry"
                />
              </div>

              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="year">
                  Year *
                </label>
                <input
                  type="number"
                  id="year"
                  name="year"
                  value={formData.year}
                  onChange={handleChange}
                  required
                  min="1900"
                  max={new Date().getFullYear() + 1}
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="2020"
                />
              </div>

              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="kilometers">
                  Kilometers *
                </label>
                <input
                  type="number"
                  id="kilometers"
                  name="kilometers"
                  value={formData.kilometers}
                  onChange={handleChange}
                  required
                  min="0"
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="50000"
                />
              </div>

              <div className="mb-4">
                <label className="block text-gray-700 font-medium mb-2" htmlFor="additionalMessage">
                  Additional Information
                </label>
                <textarea
                  id="additionalMessage"
                  name="additionalMessage"
                  value={formData.additionalMessage}
                  onChange={handleChange}
                  rows="4"
                  className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
                  style={{ focusRing: themeColor }}
                  placeholder="Any additional details about your vehicle..."
                />
              </div>
            </div>

            {/* CAPTCHA */}
            <div className="mb-6">
              <ReCaptcha ref={captchaRef} onChange={handleCaptchaChange} />
              {captchaError && (
                <p className="text-red-600 text-sm mt-1 text-center">{captchaError}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full text-white font-semibold py-3 px-6 rounded-lg hover:opacity-90 transition disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ backgroundColor: themeColor }}
            >
              {loading ? 'Submitting...' : 'Submit Request'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

export default SellYourCar;
