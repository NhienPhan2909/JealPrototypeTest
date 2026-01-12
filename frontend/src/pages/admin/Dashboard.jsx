/**
 * @fileoverview Admin Dashboard - Main admin panel landing page.
 * Displays summary statistics for selected dealership including total vehicles,
 * active listings, and recent leads (last 7 days).
 */

import { useContext, useState, useEffect } from 'react';
import AdminHeader from '../../components/AdminHeader';
import { AdminContext } from '../../context/AdminContext';
import { hasPermission } from '../../utils/permissions';

/**
 * Dashboard - Admin dashboard landing page.
 * Displays dealership summary statistics that update when selected dealership changes.
 * Fetches vehicle and lead data from API and calculates statistics.
 * All authenticated users can view dashboard statistics (read-only).
 * Permissions only restrict EDITING in specific sections.
 *
 * @component
 *
 * @example
 * <Route path="/admin" element={<Dashboard />} />
 */
export default function Dashboard() {
  const { selectedDealership, user } = useContext(AdminContext);
  const [totalVehicles, setTotalVehicles] = useState(0);
  const [activeListings, setActiveListings] = useState(0);
  const [recentLeads, setRecentLeads] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  /**
   * Fetch dashboard statistics when selected dealership changes.
   * Fetches vehicles and leads data for display purposes.
   * Dashboard is read-only - all users can view statistics.
   */
  useEffect(() => {
    const fetchDashboardStats = async () => {
      // Early return if no dealership selected
      if (!selectedDealership) {
        setLoading(false);
        return;
      }

      setLoading(true);
      setError(null);

      try {
        // Fetch vehicles for selected dealership
        const vehiclesResponse = await fetch(
          `/api/vehicles?dealershipId=${selectedDealership.id}`,
          { credentials: 'include' }
        );

        if (vehiclesResponse.ok) {
          const vehiclesData = await vehiclesResponse.json();

          // Calculate vehicle statistics
          const total = vehiclesData.length;
          const active = vehiclesData.filter(v => v.status === 'active').length;

          setTotalVehicles(total);
          setActiveListings(active);
        }

        // Fetch leads for selected dealership
        const leadsResponse = await fetch(
          `/api/leads?dealershipId=${selectedDealership.id}`,
          { credentials: 'include' }
        );

        if (leadsResponse.ok) {
          const leadsData = await leadsResponse.json();

          // Calculate recent leads (last 7 days)
          const sevenDaysAgo = new Date();
          sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
          const recentLeadsCount = leadsData.filter(lead => {
            const leadDate = new Date(lead.created_at);
            return leadDate > sevenDaysAgo;
          }).length;

          setRecentLeads(recentLeadsCount);
        }
      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardStats();
  }, [selectedDealership]);

  return (
    <div className="min-h-screen bg-gray-100">
      <AdminHeader />

      <div className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-2 text-gray-800">Dashboard</h1>

        {selectedDealership && (
          <p className="text-lg mb-6 text-gray-600">
            Managing: <span className="font-semibold">{selectedDealership.name}</span>
          </p>
        )}

        {/* Loading State */}
        {loading && (
          <div className="card">
            <p className="text-gray-700">Loading dashboard data...</p>
          </div>
        )}

        {/* Error State */}
        {error && !loading && (
          <div className="card bg-red-50 border-red-200">
            <p className="text-red-600">Error loading dashboard: {error}</p>
          </div>
        )}

        {/* Dashboard Statistics */}
        {!loading && !error && selectedDealership && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Stat Card 1: Total Vehicles */}
            <div className="card">
              <h2 className="text-xl font-semibold mb-2 text-gray-700">Total Vehicles</h2>
              <p className="text-4xl font-bold text-blue-600">{totalVehicles}</p>
            </div>

            {/* Stat Card 2: Active Listings */}
            <div className="card">
              <h2 className="text-xl font-semibold mb-2 text-gray-700">Active Listings</h2>
              <p className="text-4xl font-bold text-green-600">{activeListings}</p>
            </div>

            {/* Stat Card 3: Recent Leads */}
            <div className="card">
              <h2 className="text-xl font-semibold mb-2 text-gray-700">Recent Leads (7 days)</h2>
              <p className="text-4xl font-bold text-purple-600">{recentLeads}</p>
            </div>
          </div>
        )}

        {/* No Dealership Selected */}
        {!loading && !selectedDealership && (
          <div className="card">
            <p className="text-gray-700">Please select a dealership to view dashboard statistics.</p>
          </div>
        )}
      </div>
    </div>
  );
}
