/**
 * @fileoverview Admin Login Page - Authentication form for system access.
 * Handles username/password validation and redirects to dashboard on success.
 */

import { useState, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { AdminContext } from '../../context/AdminContext';
import apiRequest from '../../utils/api';

/**
 * Login - Login page component for all user types.
 * Displays login form with username/password fields and handles authentication.
 * On successful login, updates AdminContext and redirects to /admin dashboard.
 *
 * @component
 *
 * @example
 * <Route path="/admin/login" element={<Login />} />
 */
export default function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { setIsAuthenticated, setUser } = useContext(AdminContext);
  const navigate = useNavigate();

  /**
   * Handle form submission - POST to /api/auth/login.
   * @param {Event} e - Form submit event
   */
  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      const response = await apiRequest('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ username, password })
      });

      const result = await response.json();
      const data = result.data || result.Data || result;

      if (response.ok) {
        // Success - store token and update auth state
        if (data.token) {
          localStorage.setItem('jwt_token', data.token);
        }
        setIsAuthenticated(true);
        setUser(data.user);
        navigate('/admin');
      } else {
        // Error - display error message
        setError(result.message || result.Message || 'Invalid username or password');
      }
    } catch (err) {
      console.error('Login error:', err);
      setError('Failed to connect to server. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="max-w-md w-full mx-4">
        <div className="card">
          <h1 className="text-3xl font-bold mb-6 text-center">Login</h1>

          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label htmlFor="username" className="block text-sm font-medium mb-2">
                Username
              </label>
              <input
                id="username"
                type="text"
                className="input-field"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                autoFocus
                disabled={isSubmitting}
              />
            </div>

            <div className="mb-4">
              <label htmlFor="password" className="block text-sm font-medium mb-2">
                Password
              </label>
              <input
                id="password"
                type="password"
                className="input-field"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                disabled={isSubmitting}
              />
            </div>

            {error && (
              <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
                {error}
              </div>
            )}

            <button
              type="submit"
              className="btn-primary w-full"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Logging in...' : 'Log In'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
