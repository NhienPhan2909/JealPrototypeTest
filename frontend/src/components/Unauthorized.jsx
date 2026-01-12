/**
 * @fileoverview Unauthorized access component.
 * Shows when a user tries to access a section they don't have permission for.
 */

import { Link } from 'react-router-dom';
import AdminHeader from './AdminHeader';

/**
 * Unauthorized - Component shown when user lacks permission.
 * 
 * @component
 * @param {Object} props
 * @param {string} props.section - Name of section user tried to access
 */
export default function Unauthorized({ section = 'this section' }) {
  return (
    <div className="min-h-screen bg-gray-50">
      <AdminHeader />
      <div className="max-w-4xl mx-auto p-8">
        <div className="card text-center">
          <div className="mb-6">
            <svg 
              className="mx-auto h-24 w-24 text-red-500" 
              fill="none" 
              viewBox="0 0 24 24" 
              stroke="currentColor"
            >
              <path 
                strokeLinecap="round" 
                strokeLinejoin="round" 
                strokeWidth={2} 
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" 
              />
            </svg>
          </div>
          
          <h1 className="text-3xl font-bold text-gray-900 mb-4">
            Access Denied
          </h1>
          
          <p className="text-xl text-gray-700 mb-6">
            You cannot access {section} without proper authorization.
          </p>
          
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-6">
            <p className="text-sm text-gray-700">
              <strong>Need access?</strong> Contact your dealership owner to request the necessary permissions.
            </p>
          </div>
          
          <div className="flex gap-4 justify-center">
            <Link 
              to="/admin" 
              className="btn-primary"
            >
              ← Back to Dashboard
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
