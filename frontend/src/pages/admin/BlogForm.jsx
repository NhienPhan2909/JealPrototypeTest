/**
 * @fileoverview BlogForm - Admin blog post create/edit form component.
 * Handles both create and edit modes with featured image uploads.
 *
 * SECURITY (SEC-001): All API calls include dealershipId parameter for multi-tenant data isolation.
 * Edit mode requires dealershipId query parameter to prevent cross-dealership modifications.
 *
 * Features:
 * - Create mode: /admin/blogs/new (no blog data loaded)
 * - Edit mode: /admin/blogs/edit/:id (pre-populates with existing blog data)
 * - React Hook Form for form state and validation
 * - Featured image upload with preview
 * - Rich text editor for content
 * - Auto-generate slug from title
 * - Status selection (draft, published, archived)
 *
 * @component
 */

import { useState, useEffect, useContext } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';
import Unauthorized from '../../components/Unauthorized';
import { hasPermission } from '../../utils/permissions';

/**
 * BlogForm - Create and edit blog post form component.
 * Requires 'blogs' permission to access.
 */
function BlogForm() {
  const navigate = useNavigate();
  const { user } = useContext(AdminContext);

  // Check permission
  if (!hasPermission(user, 'blogs')) {
    return <Unauthorized section="Blog Manager" />;
  }
  const { id } = useParams();
  const { selectedDealership } = useContext(AdminContext);
  const isEditMode = Boolean(id);

  // React Hook Form setup
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
    setValue
  } = useForm({
    defaultValues: {
      title: '',
      slug: '',
      content: '',
      excerpt: '',
      author_name: '',
      status: 'draft'
    }
  });

  // Component state
  const [featuredImage, setFeaturedImage] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetchingBlog, setFetchingBlog] = useState(false);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  const [uploading, setUploading] = useState(false);

  // Watch title for auto-slug generation
  const watchTitle = watch('title');
  const watchSlug = watch('slug');

  /**
   * Auto-generate slug from title if slug is empty.
   */
  useEffect(() => {
    if (watchTitle && !watchSlug && !isEditMode) {
      const generatedSlug = watchTitle
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '')
        .substring(0, 200);
      setValue('slug', generatedSlug);
    }
  }, [watchTitle, watchSlug, setValue, isEditMode]);

  /**
   * Fetches existing blog post data for edit mode.
   */
  useEffect(() => {
    const fetchBlog = async () => {
      if (!isEditMode || !selectedDealership) return;

      try {
        setFetchingBlog(true);
        setError(null);

        const response = await fetch(
          `/api/blogs/${id}?dealershipId=${selectedDealership.id}`,
          { credentials: 'include' }
        );

        if (!response.ok) {
          if (response.status === 404) {
            throw new Error('Blog post not found or does not belong to this dealership');
          }
          throw new Error('Failed to fetch blog post data');
        }

        const blog = await response.json();

        reset({
          title: blog.title,
          slug: blog.slug,
          content: blog.content,
          excerpt: blog.excerpt || '',
          author_name: blog.author_name,
          status: blog.status
        });

        setFeaturedImage(blog.featured_image_url || '');
      } catch (err) {
        console.error('Error fetching blog post:', err);
        setError(err.message);
      } finally {
        setFetchingBlog(false);
      }
    };

    fetchBlog();
  }, [id, isEditMode, selectedDealership, reset]);

  /**
   * Handles featured image upload.
   */
  const handleImageUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/jpg'];
    if (!allowedTypes.includes(file.type)) {
      setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
      e.target.value = '';
      return;
    }

    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      setError('File is too large. Maximum size is 5MB.');
      e.target.value = '';
      return;
    }

    setUploading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', file);

      const response = await fetch('/api/upload', {
        method: 'POST',
        credentials: 'include',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Upload failed');
      }

      const data = await response.json();
      setFeaturedImage(data.url);
      setSuccessMessage('Image uploaded successfully!');

      setTimeout(() => {
        setSuccessMessage('');
      }, 3000);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload image: ' + err.message);
    } finally {
      setUploading(false);
      e.target.value = '';
    }
  };

  /**
   * Removes featured image.
   */
  const removeFeaturedImage = () => {
    setFeaturedImage('');
  };

  /**
   * Handles form submission for both create and edit modes.
   */
  const onSubmit = async (data) => {
    if (!selectedDealership) {
      setError('No dealership selected');
      return;
    }

    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    try {
      const blogData = {
        ...data,
        featured_image_url: featuredImage || null
      };

      let response;

      if (isEditMode) {
        // Edit mode: PUT request with dealershipId query parameter
        response = await fetch(
          `/api/blogs/${id}?dealershipId=${selectedDealership.id}`,
          {
            method: 'PUT',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(blogData)
          }
        );
      } else {
        // Create mode: POST request with dealership_id in body
        blogData.dealership_id = selectedDealership.id;
        response = await fetch('/api/blogs', {
          method: 'POST',
          credentials: 'include',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(blogData)
        });
      }

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Failed to save blog post');
      }

      setSuccessMessage(
        isEditMode
          ? 'Blog post updated successfully!'
          : 'Blog post created successfully!'
      );

      setTimeout(() => {
        navigate('/admin/blogs');
      }, 1500);
    } catch (err) {
      console.error('Error saving blog post:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handles cancel button click.
   */
  const handleCancel = () => {
    navigate('/admin/blogs');
  };

  // Loading state
  if (fetchingBlog) {
    return (
      <div>
        <AdminHeader />
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <p className="text-gray-600">Loading blog post...</p>
        </div>
      </div>
    );
  }

  // No dealership selected
  if (!selectedDealership) {
    return (
      <div>
        <AdminHeader />
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <p className="text-gray-600">Please select a dealership to manage blog posts.</p>
        </div>
      </div>
    );
  }

  return (
    <div>
      <AdminHeader />
      
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            {isEditMode ? 'Edit Blog Post' : 'Add New Blog Post'}
          </h1>
          <p className="mt-2 text-gray-600">
            {isEditMode 
              ? 'Update the blog post information below'
              : 'Fill in the details to create a new blog post'}
          </p>
        </div>

        {/* Messages */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded mb-6">
            {error}
          </div>
        )}

        {successMessage && (
          <div className="bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded mb-6">
            {successMessage}
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div className="bg-white shadow-md rounded-lg p-6 space-y-6">
            {/* Title */}
            <div>
              <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-2">
                Title *
              </label>
              <input
                id="title"
                type="text"
                {...register('title', { required: 'Title is required' })}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter blog post title"
              />
              {errors.title && (
                <p className="text-red-600 text-sm mt-1">{errors.title.message}</p>
              )}
            </div>

            {/* Slug */}
            <div>
              <label htmlFor="slug" className="block text-sm font-medium text-gray-700 mb-2">
                Slug (URL-friendly) *
              </label>
              <input
                id="slug"
                type="text"
                {...register('slug', { 
                  required: 'Slug is required',
                  pattern: {
                    value: /^[a-z0-9-]+$/,
                    message: 'Slug can only contain lowercase letters, numbers, and hyphens'
                  }
                })}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="auto-generated-from-title"
              />
              {errors.slug && (
                <p className="text-red-600 text-sm mt-1">{errors.slug.message}</p>
              )}
              <p className="text-gray-500 text-sm mt-1">
                This will be used in the URL: /blog/{watchSlug || 'your-slug'}
              </p>
            </div>

            {/* Excerpt */}
            <div>
              <label htmlFor="excerpt" className="block text-sm font-medium text-gray-700 mb-2">
                Excerpt
              </label>
              <textarea
                id="excerpt"
                {...register('excerpt')}
                rows={3}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Short summary of the blog post (optional)"
              />
              <p className="text-gray-500 text-sm mt-1">
                Brief description shown in blog listing
              </p>
            </div>

            {/* Content */}
            <div>
              <label htmlFor="content" className="block text-sm font-medium text-gray-700 mb-2">
                Content *
              </label>
              <textarea
                id="content"
                {...register('content', { required: 'Content is required' })}
                rows={12}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-mono text-sm"
                placeholder="Write your blog post content here..."
              />
              {errors.content && (
                <p className="text-red-600 text-sm mt-1">{errors.content.message}</p>
              )}
            </div>

            {/* Author Name */}
            <div>
              <label htmlFor="author_name" className="block text-sm font-medium text-gray-700 mb-2">
                Author Name *
              </label>
              <input
                id="author_name"
                type="text"
                {...register('author_name', { required: 'Author name is required' })}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter author name"
              />
              {errors.author_name && (
                <p className="text-red-600 text-sm mt-1">{errors.author_name.message}</p>
              )}
            </div>

            {/* Featured Image */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Featured Image
              </label>
              
              {featuredImage ? (
                <div className="space-y-4">
                  <img
                    src={featuredImage}
                    alt="Featured"
                    className="w-full max-w-2xl h-64 object-cover rounded-lg border border-gray-300"
                  />
                  <button
                    type="button"
                    onClick={removeFeaturedImage}
                    className="text-red-600 hover:text-red-800 text-sm font-medium"
                  >
                    Remove Image
                  </button>
                </div>
              ) : (
                <div>
                  <input
                    type="file"
                    accept="image/jpeg,image/png,image/webp,image/jpg"
                    onChange={handleImageUpload}
                    disabled={uploading}
                    className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                  {uploading && (
                    <p className="text-blue-600 text-sm mt-2">Uploading...</p>
                  )}
                </div>
              )}
            </div>

            {/* Status */}
            <div>
              <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-2">
                Status *
              </label>
              <select
                id="status"
                {...register('status', { required: 'Status is required' })}
                className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="draft">Draft</option>
                <option value="published">Published</option>
                <option value="archived">Archived</option>
              </select>
              {errors.status && (
                <p className="text-red-600 text-sm mt-1">{errors.status.message}</p>
              )}
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end gap-4">
            <button
              type="button"
              onClick={handleCancel}
              className="px-6 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-blue-300"
              disabled={loading}
            >
              {loading ? 'Saving...' : isEditMode ? 'Update Blog Post' : 'Create Blog Post'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default BlogForm;
