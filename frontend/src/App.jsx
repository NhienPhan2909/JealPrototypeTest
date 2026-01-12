/**
 * @fileoverview Root application component with React Router configuration.
 * Defines routes for public pages (Home, Inventory, VehicleDetail, About) and admin routes.
 * Wraps app with AdminProvider for global authentication state.
 */

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AdminProvider } from './context/AdminContext';
import { DealershipProvider } from './context/DealershipContext';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import Home from './pages/public/Home';
import Inventory from './pages/public/Inventory';
import VehicleDetail from './pages/public/VehicleDetail';
import About from './pages/public/About';
import Finance from './pages/public/Finance';
import Warranty from './pages/public/Warranty';
import Location from './pages/public/Location';
import SellYourCar from './pages/public/SellYourCar';
import Blog from './pages/public/Blog';
import BlogPost from './pages/public/BlogPost';
import Login from './pages/admin/Login';
import Dashboard from './pages/admin/Dashboard';
import VehicleList from './pages/admin/VehicleList';
import VehicleForm from './pages/admin/VehicleForm';
import LeadInbox from './pages/admin/LeadInbox';
import SalesRequests from './pages/admin/SalesRequests';
import DealerSettings from './pages/admin/DealerSettings';
import BlogList from './pages/admin/BlogList';
import BlogForm from './pages/admin/BlogForm';
import UserManagement from './pages/admin/UserManagement';

/**
 * App - Root component with routing configuration.
 *
 * @component
 *
 * Sets up React Router with nested routes:
 * - Public routes wrapped in Layout component (shared header/footer)
 * - Admin login route (public access)
 * - Protected admin routes wrapped in ProtectedRoute (requires authentication)
 *
 * AdminProvider wraps entire app to provide global authentication state via Context API.
 *
 * @example
 * <App />
 */
function App() {
  return (
    <AdminProvider>
      <DealershipProvider>
        <BrowserRouter>
          <Routes>
          {/* Public routes */}
          <Route path="/" element={<Layout />}>
            <Route index element={<Home />} />
            <Route path="inventory" element={<Inventory />} />
            <Route path="inventory/:vehicleId" element={<VehicleDetail />} />
            <Route path="about" element={<About />} />
            <Route path="finance" element={<Finance />} />
            <Route path="warranty" element={<Warranty />} />
            <Route path="location" element={<Location />} />
            <Route path="sell-your-car" element={<SellYourCar />} />
            <Route path="blog" element={<Blog />} />
            <Route path="blog/:slug" element={<BlogPost />} />
          </Route>

          {/* Admin login (public access) */}
          <Route path="/admin/login" element={<Login />} />

          {/* Protected admin routes */}
          <Route path="/admin" element={<ProtectedRoute />}>
            <Route index element={<Dashboard />} />
            <Route path="vehicles" element={<VehicleList />} />
            <Route path="vehicles/new" element={<VehicleForm />} />
            <Route path="vehicles/edit/:id" element={<VehicleForm />} />
            <Route path="leads" element={<LeadInbox />} />
            <Route path="sales-requests" element={<SalesRequests />} />
            <Route path="settings" element={<DealerSettings />} />
            <Route path="blogs" element={<BlogList />} />
            <Route path="blogs/new" element={<BlogForm />} />
            <Route path="blogs/edit/:id" element={<BlogForm />} />
            <Route path="users" element={<UserManagement />} />
          </Route>
          </Routes>
        </BrowserRouter>
      </DealershipProvider>
    </AdminProvider>
  );
}

export default App;
