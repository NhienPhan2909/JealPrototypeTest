/**
 * @fileoverview ProtectedRoute - Authentication wrapper for admin routes.
 * Redirects unauthenticated users to login page.
 */

import { useContext } from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { AdminContext } from '../context/AdminContext';

/**
 * ProtectedRoute - Wrapper component for admin-only routes.
 * Checks authentication status and redirects to /admin/login if not authenticated.
 * Renders nested routes via <Outlet /> if authenticated.
 *
 * @component
 *
 * @example
 * <Route path="/admin" element={<ProtectedRoute />}>
 *   <Route index element={<Dashboard />} />
 *   <Route path="vehicles" element={<VehicleList />} />
 * </Route>
 */
export default function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useContext(AdminContext);

  // Show loading state while checking authentication
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-lg">Loading...</div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/admin/login" replace />;
  }

  // Render nested routes if authenticated
  return <Outlet />;
}
