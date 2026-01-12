/**
 * @fileoverview User Management Page - CRUD interface for managing user accounts.
 * Admin can manage dealership owners, owners can manage their staff.
 */

import { useState, useEffect, useContext } from 'react';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';

export default function UserManagement() {
  const { user } = useContext(AdminContext);
  const [users, setUsers] = useState([]);
  const [dealerships, setDealerships] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingUser, setEditingUser] = useState(null);

  // Form state
  const [formData, setFormData] = useState({
    username: '',
    password: '',
    email: '',
    full_name: '',
    user_type: 'dealership_staff',
    dealership_id: '',
    permissions: []
  });

  const permissionOptions = [
    { value: 'leads', label: 'Manage Leads' },
    { value: 'sales_requests', label: 'Manage Sales Requests' },
    { value: 'vehicles', label: 'Manage Vehicles' },
    { value: 'blogs', label: 'Manage Blogs' },
    { value: 'settings', label: 'Edit Dealership Settings' }
  ];

  useEffect(() => {
    fetchUsers();
    if (user?.user_type === 'admin') {
      fetchDealerships();
    }
  }, [user]);

  const fetchUsers = async () => {
    try {
      const response = await fetch('/api/users', { credentials: 'include' });
      if (response.ok) {
        const data = await response.json();
        setUsers(data);
      } else {
        setError('Failed to load users');
      }
    } catch (err) {
      console.error('Fetch users error:', err);
      setError('Failed to load users');
    } finally {
      setIsLoading(false);
    }
  };

  const fetchDealerships = async () => {
    try {
      const response = await fetch('/api/dealers', { credentials: 'include' });
      if (response.ok) {
        const data = await response.json();
        setDealerships(data);
      }
    } catch (err) {
      console.error('Fetch dealerships error:', err);
    }
  };

  const handleCreateUser = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const payload = { ...formData };
      
      // Set dealership_id based on user type
      if (user.user_type === 'dealership_owner') {
        payload.dealership_id = user.dealership_id;
        payload.user_type = 'dealership_staff';
      } else if (payload.user_type === 'admin') {
        payload.dealership_id = null;
      }

      const response = await fetch('/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        await fetchUsers();
        setShowCreateForm(false);
        resetForm();
      } else {
        const data = await response.json();
        setError(data.error || 'Failed to create user');
      }
    } catch (err) {
      console.error('Create user error:', err);
      setError('Failed to create user');
    }
  };

  const handleUpdateUser = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const response = await fetch(`/api/users/${editingUser.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          email: formData.email,
          full_name: formData.full_name,
          permissions: formData.permissions
        })
      });

      if (response.ok) {
        await fetchUsers();
        setEditingUser(null);
        resetForm();
      } else {
        const data = await response.json();
        setError(data.error || 'Failed to update user');
      }
    } catch (err) {
      console.error('Update user error:', err);
      setError('Failed to update user');
    }
  };

  const handleDeleteUser = async (userId) => {
    if (!confirm('Are you sure you want to delete this user? This action cannot be undone.')) return;

    try {
      const response = await fetch(`/api/users/${userId}`, {
        method: 'DELETE',
        credentials: 'include'
      });

      if (response.ok) {
        await fetchUsers();
      } else {
        const data = await response.json();
        alert(data.error || 'Failed to delete user');
      }
    } catch (err) {
      console.error('Delete user error:', err);
      alert('Failed to delete user');
    }
  };

  const startEdit = (userToEdit) => {
    setEditingUser(userToEdit);
    setFormData({
      username: userToEdit.username,
      password: '',
      email: userToEdit.email,
      full_name: userToEdit.full_name,
      user_type: userToEdit.user_type,
      dealership_id: userToEdit.dealership_id || '',
      permissions: userToEdit.permissions || []
    });
    setShowCreateForm(false);
  };

  const resetForm = () => {
    setFormData({
      username: '',
      password: '',
      email: '',
      full_name: '',
      user_type: user?.user_type === 'admin' ? 'dealership_owner' : 'dealership_staff',
      dealership_id: user?.user_type === 'dealership_owner' ? user.dealership_id : '',
      permissions: []
    });
  };

  const handlePermissionToggle = (permission) => {
    setFormData(prev => ({
      ...prev,
      permissions: prev.permissions.includes(permission)
        ? prev.permissions.filter(p => p !== permission)
        : [...prev.permissions, permission]
    }));
  };

  if (isLoading) {
    return <div className="p-8">Loading...</div>;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <AdminHeader />
      <div className="max-w-6xl mx-auto p-8">
        <div className="mb-6 flex justify-between items-center">
          <h1 className="text-3xl font-bold">User Management</h1>
          {!showCreateForm && !editingUser && (
            <button
              onClick={() => {
                setShowCreateForm(true);
                resetForm();
              }}
              className="btn-primary"
            >
              + Create User
            </button>
          )}
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        {/* Create/Edit Form */}
        {(showCreateForm || editingUser) && (
          <div className="card mb-6">
            <h2 className="text-xl font-bold mb-4">
              {editingUser ? 'Edit User' : 'Create New User'}
            </h2>
            <form onSubmit={editingUser ? handleUpdateUser : handleCreateUser}>
              {!editingUser && (
                <div className="mb-4">
                  <label className="block text-sm font-medium mb-2">Username *</label>
                  <input
                    type="text"
                    className="input-field"
                    value={formData.username}
                    onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                    required
                  />
                </div>
              )}

              {!editingUser && (
                <div className="mb-4">
                  <label className="block text-sm font-medium mb-2">Password *</label>
                  <input
                    type="password"
                    className="input-field"
                    value={formData.password}
                    onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                    required
                    minLength={6}
                  />
                </div>
              )}

              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">Email *</label>
                <input
                  type="email"
                  className="input-field"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  required
                />
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">Full Name *</label>
                <input
                  type="text"
                  className="input-field"
                  value={formData.full_name}
                  onChange={(e) => setFormData({ ...formData, full_name: e.target.value })}
                  required
                />
              </div>

              {!editingUser && user?.user_type === 'admin' && (
                <>
                  <div className="mb-4">
                    <label className="block text-sm font-medium mb-2">User Type *</label>
                    <select
                      className="input-field"
                      value={formData.user_type}
                      onChange={(e) => setFormData({ ...formData, user_type: e.target.value })}
                      required
                    >
                      <option value="dealership_owner">Dealership Owner</option>
                      <option value="admin">Admin</option>
                    </select>
                  </div>

                  {formData.user_type === 'dealership_owner' && (
                    <div className="mb-4">
                      <label className="block text-sm font-medium mb-2">Dealership *</label>
                      <select
                        className="input-field"
                        value={formData.dealership_id}
                        onChange={(e) => setFormData({ ...formData, dealership_id: parseInt(e.target.value) })}
                        required
                      >
                        <option value="">Select Dealership</option>
                        {dealerships.map(d => (
                          <option key={d.id} value={d.id}>{d.name}</option>
                        ))}
                      </select>
                    </div>
                  )}
                </>
              )}

              {(formData.user_type === 'dealership_staff' || editingUser?.user_type === 'dealership_staff') && (
                <div className="mb-4">
                  <label className="block text-sm font-medium mb-2">Permissions</label>
                  <div className="space-y-2">
                    {permissionOptions.map(option => (
                      <label key={option.value} className="flex items-center">
                        <input
                          type="checkbox"
                          checked={formData.permissions.includes(option.value)}
                          onChange={() => handlePermissionToggle(option.value)}
                          className="mr-2"
                        />
                        {option.label}
                      </label>
                    ))}
                  </div>
                </div>
              )}

              <div className="flex gap-2">
                <button type="submit" className="btn-primary">
                  {editingUser ? 'Update User' : 'Create User'}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowCreateForm(false);
                    setEditingUser(null);
                    resetForm();
                  }}
                  className="btn-secondary"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        )}

        {/* Users List */}
        <div className="card">
          <h2 className="text-xl font-bold mb-4">Users</h2>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Username</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                  {user?.user_type === 'admin' && (
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Dealership</th>
                  )}
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Permissions</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {users.map(u => (
                  <tr key={u.id}>
                    <td className="px-6 py-4 whitespace-nowrap">{u.full_name}</td>
                    <td className="px-6 py-4 whitespace-nowrap">{u.username}</td>
                    <td className="px-6 py-4 whitespace-nowrap">{u.email}</td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="px-2 py-1 text-xs rounded bg-blue-100 text-blue-800">
                        {u.user_type.replace('_', ' ')}
                      </span>
                    </td>
                    {user?.user_type === 'admin' && (
                      <td className="px-6 py-4 whitespace-nowrap">{u.dealership_name || '-'}</td>
                    )}
                    <td className="px-6 py-4">
                      {u.user_type === 'dealership_staff' ? (
                        u.permissions?.length > 0 ? u.permissions.join(', ') : 'Read-only'
                      ) : (
                        'Full access'
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {u.id !== user.id && (
                        <div className="flex gap-2">
                          <button
                            onClick={() => startEdit(u)}
                            className="text-blue-600 hover:text-blue-800"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDeleteUser(u.id)}
                            className="text-red-600 hover:text-red-800"
                          >
                            Delete
                          </button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
