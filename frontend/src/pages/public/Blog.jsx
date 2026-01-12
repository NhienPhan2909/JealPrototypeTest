/**
 * @fileoverview Blog - Public blog listing page.
 * Displays published blog posts for the dealership.
 *
 * Features:
 * - Fetches and displays published blog posts
 * - Grid layout with featured images and excerpts
 * - Click to read full post
 * - Loading and error states
 *
 * @component
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDealershipContext } from '../../context/DealershipContext';

/**
 * Blog - Public blog listing page component.
 */
function Blog() {
  const { currentDealershipId, currentDealership } = useDealershipContext();
  const navigate = useNavigate();

  const [blogs, setBlogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  /**
   * Fetches published blog posts from API.
   */
  useEffect(() => {
    const fetchBlogs = async () => {
      if (!currentDealershipId) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const response = await fetch(
          `/api/blogs/published?dealershipId=${currentDealershipId}`
        );

        if (!response.ok) {
          throw new Error('Failed to fetch blog posts');
        }

        const data = await response.json();
        setBlogs(data);
      } catch (err) {
        console.error('Error fetching blog posts:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchBlogs();
  }, [currentDealershipId]);

  /**
   * Formats date string for display.
   */
  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  /**
   * Handles blog post click.
   */
  const handleBlogClick = (slug) => {
    navigate(`/blog/${slug}`);
  };

  // Loading state
  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <p className="text-gray-600 text-center">Loading blog posts...</p>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded">
          <p className="font-medium">Error loading blog posts</p>
          <p className="text-sm">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="mb-12 text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          {currentDealership?.name || 'Our'} Blog
        </h1>
        <p className="text-xl text-gray-600">
          Stay updated with the latest news, tips, and insights
        </p>
      </div>

      {/* Blog Posts Grid */}
      {blogs.length === 0 ? (
        <div className="bg-gray-50 border border-gray-200 rounded-lg p-12 text-center">
          <p className="text-gray-600 text-lg">
            No blog posts available at the moment. Check back soon!
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {blogs.map((blog) => (
            <article
              key={blog.id}
              onClick={() => handleBlogClick(blog.slug)}
              className="bg-white rounded-lg shadow-md overflow-hidden cursor-pointer hover:shadow-xl transition-shadow duration-300"
            >
              {/* Featured Image */}
              {blog.featured_image_url ? (
                <img
                  src={blog.featured_image_url}
                  alt={blog.title}
                  className="w-full h-48 object-cover"
                />
              ) : (
                <div className="w-full h-48 bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center">
                  <span className="text-white text-6xl">📝</span>
                </div>
              )}

              {/* Content */}
              <div className="p-6">
                <h2 className="text-xl font-bold text-gray-900 mb-2 hover:text-blue-600 transition-colors">
                  {blog.title}
                </h2>
                
                <div className="flex items-center text-sm text-gray-500 mb-3">
                  <span>{blog.author_name}</span>
                  <span className="mx-2">•</span>
                  <span>{formatDate(blog.published_at)}</span>
                </div>

                {blog.excerpt && (
                  <p className="text-gray-600 mb-4 line-clamp-3">
                    {blog.excerpt}
                  </p>
                )}

                <span className="text-blue-600 font-medium hover:text-blue-700">
                  Read More →
                </span>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}

export default Blog;
