/**
 * @fileoverview API utility for making HTTP requests to the .NET backend.
 * Provides a centralized fetch wrapper with base URL configuration.
 */

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

/**
 * Makes an HTTP request to the API with the configured base URL.
 * 
 * @param {string} endpoint - API endpoint (e.g., '/api/auth/login')
 * @param {RequestInit} options - Fetch options
 * @returns {Promise<Response>} Fetch response
 */
export async function apiRequest(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  
  const token = localStorage.getItem('jwt_token');
  
  // Don't set Content-Type for FormData - browser will set it with boundary
  const isFormData = options.body instanceof FormData;
  
  const defaultHeaders = {
    ...(!isFormData && { 'Content-Type': 'application/json' }),
    ...(token && { 'Authorization': `Bearer ${token}` }),
  };

  const mergedOptions = {
    ...options,
    credentials: 'include',
    headers: {
      ...defaultHeaders,
      ...options.headers,
    },
  };

  const response = await fetch(url, mergedOptions);
  return response;
}

/**
 * Helper to get the full API URL for a given endpoint.
 * 
 * @param {string} endpoint - API endpoint
 * @returns {string} Full URL
 */
export function getApiUrl(endpoint) {
  return `${API_BASE_URL}${endpoint}`;
}

export default apiRequest;
