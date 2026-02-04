/**
 * @fileoverview Blog Manager List View - Admin page for managing dealership blog posts.
 * Displays blog posts in table format with Edit/Delete actions, status filtering, and delete confirmation.
 *
 * SECURITY (SEC-001): All API calls include dealershipId parameter for multi-tenant data isolation.
 * Only blog posts belonging to selected dealership are displayed and can be modified.
 *
 * Features:
 * - Fetches and displays blog posts for selected dealership
 * - Table view with columns: Title, Author, Status, Published Date, Actions
 * - Status filter dropdown (All, Draft, Published, Archived)
 * - Delete confirmation modal with ownership validation
 * - Navigation to Add/Edit blog post forms
 * - Loading, error, and empty states
 *
 * @component
 */

import { useState, useEffect, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';
import Unauthorized from '../../components/Unauthorized';
import { hasPermission } from '../../utils/permissions';
import apiRequest from '../../utils/api';

/**
 * BlogList - Admin blog manager page component.
 * Requires 'blogs' permission to access.
 */
export default function BlogList() {
  const navigate = useNavigate();
  const { selectedDealership, user } = useContext(AdminContext);

  // Check if user can edit blogs
  const canEditBlogs = hasPermission(user, 'blogs');

  // State management
  const [blogs, setBlogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [statusFilter, setStatusFilter] = useState('All');
  const [deleteModal, setDeleteModal] = useState({
    isOpen: false,
    blogId: null,
    blogTitle: ''
  });

  /**
   * Fetches blog posts for selected dealership from API.
   * Triggered on component mount and when selectedDealership changes.
   *
   * SECURITY: Includes dealershipId query parameter for multi-tenant filtering (SEC-001).
   */
  useEffect(() => {
    const fetchBlogs = async () => {
      if (!selectedDealership) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const response = await apiRequest(
          `/api/blog-posts/dealership/${selectedDealership.id}`
        );

        if (!response.ok) {
          throw new Error('Failed to fetch blog posts');
        }

        const result = await response.json();
        setBlogs(result.data || result);
      } catch (err) {
        console.error('Error fetching blog posts:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchBlogs();
  }, [selectedDealership]);

  /**
   * Filters blog posts based on selected status.
   */
  const filteredBlogs = statusFilter === 'All'
    ? blogs
    : blogs.filter(blog => blog.status === statusFilter.toLowerCase());

  /**
   * Navigates to Add Blog Post form.
   */
  const handleAddBlog = () => {
    navigate('/admin/blogs/new');
  };

  /**
   * Navigates to Edit Blog Post form for specified blog.
   */
  const handleEdit = (blogId) => {
    navigate(`/admin/blogs/edit/${blogId}`);
  };

  /**
   * Opens delete confirmation modal for specified blog post.
   */
  const openDeleteModal = (blog) => {
    setDeleteModal({
      isOpen: true,
      blogId: blog.id,
      blogTitle: blog.title
    });
  };

  /**
   * Closes delete confirmation modal without action.
   */
  const closeDeleteModal = () => {
    setDeleteModal({
      isOpen: false,
      blogId: null,
      blogTitle: ''
    });
  };

  /**
   * Deletes blog post after confirmation.
   */
  const confirmDelete = async (blogId) => {
    try {
      const response = await apiRequest(
        `/api/blog-posts/${blogId}?dealershipId=${selectedDealership.id}`,
        {
          method: 'DELETE'
        }
      );

      if (!response.ok) {
        throw new Error('Failed to delete blog post');
      }

      // Remove deleted blog from state
      setBlogs(blogs.filter(blog => blog.id !== blogId));
      closeDeleteModal();
    } catch (err) {
      console.error('Error deleting blog post:', err);
      alert('Failed to delete blog post. Please try again.');
    }
  };

  /**
   * Formats date string for display.
   */
  const formatDate = (dateString) => {
    if (!dateString) return 'Not published';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  // Loading state
  if (loading) {
    return (
      <div>
        <AdminHeader />
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <p className="text-gray-600">Loading blog posts...</p>
        </div>
      </div>
    );
  }

  // No dealership selected state
  if (!selectedDealership) {
    return (
      <div>
        <AdminHeader />
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <p className="text-gray-600">Please select a dealership to manage blog posts.</p>
        </div>
      </div>
    );
  }

  return (
    <div>
      <AdminHeader />
      
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Read-only banner for users without edit permission */}
        {!canEditBlogs && (
          <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6">
            <div className="flex">
              <div className="flex-shrink-0">
                <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                </svg>
              </div>
              <div className="ml-3">
                <p className="text-sm text-yellow-700">
                  <strong>View Only:</strong> You can view all blog posts but cannot create, edit, or delete them. Contact your dealership owner to request blog management permission.
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Header Section */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Blog Manager</h1>
          <p className="mt-2 text-gray-600">
            Manage blog posts for {selectedDealership.name}
          </p>
        </div>

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded mb-6">
            <p className="font-medium">Error loading blog posts</p>
            <p className="text-sm">{error}</p>
          </div>
        )}

        {/* Actions Bar */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          {canEditBlogs && (
            <button
              onClick={handleAddBlog}
              className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition-colors font-medium"
            >
              + Add New Blog Post
            </button>
          )}

          <div className="flex items-center gap-2">
            <label htmlFor="statusFilter" className="text-gray-700 font-medium">
              Filter by Status:
            </label>
            <select
              id="statusFilter"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="All">All</option>
              <option value="Draft">Draft</option>
              <option value="Published">Published</option>
              <option value="Archived">Archived</option>
            </select>
          </div>
        </div>

        {/* Blog Posts Table */}
        {filteredBlogs.length === 0 ? (
          <div className="bg-gray-50 border border-gray-200 rounded-lg p-12 text-center">
            <p className="text-gray-600 text-lg">
              {statusFilter === 'All' 
                ? (canEditBlogs 
                    ? 'No blog posts yet. Click "Add New Blog Post" to create your first post.'
                    : 'No blog posts available for this dealership.')
                : `No ${statusFilter.toLowerCase()} blog posts found.`}
            </p>
          </div>
        ) : (
          <div className="bg-white shadow-md rounded-lg overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Title
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Author
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Published Date
                  </th>
                  {canEditBlogs && (
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  )}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredBlogs.map((blog) => (
                  <tr key={blog.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">{blog.title}</div>
                      {blog.excerpt && (
                        <div className="text-sm text-gray-500 truncate max-w-md">
                          {blog.excerpt}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {blog.authorName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full
                        ${blog.status === 'published' ? 'bg-green-100 text-green-800' : ''}
                        ${blog.status === 'draft' ? 'bg-yellow-100 text-yellow-800' : ''}
                        ${blog.status === 'archived' ? 'bg-gray-100 text-gray-800' : ''}
                      `}>
                        {blog.status.charAt(0).toUpperCase() + blog.status.slice(1)}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {formatDate(blog.published_at)}
                    </td>
                    {canEditBlogs && (
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button
                          onClick={() => handleEdit(blog.id)}
                          className="text-blue-600 hover:text-blue-900 mr-4"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => openDeleteModal(blog)}
                          className="text-red-600 hover:text-red-900"
                        >
                          Delete
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Delete Confirmation Modal */}
      {deleteModal.isOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Confirm Delete</h2>
            <p className="text-gray-600 mb-6">
              Are you sure you want to delete "<strong>{deleteModal.blogTitle}</strong>"? 
              This action cannot be undone.
            </p>
            <div className="flex justify-end gap-4">
              <button
                onClick={closeDeleteModal}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={() => confirmDelete(deleteModal.blogId)}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
