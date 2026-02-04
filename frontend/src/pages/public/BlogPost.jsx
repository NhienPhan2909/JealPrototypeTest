/**
 * @fileoverview BlogPost - Public single blog post page.
 * Displays the full content of a single blog post.
 *
 * Features:
 * - Fetches blog post by slug from URL
 * - Displays full content with featured image
 * - Shows author and published date
 * - Back to blog list navigation
 * - Loading and error states
 *
 * @component
 */

import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useDealershipContext } from '../../context/DealershipContext';
import apiRequest from '../../utils/api';

/**
 * BlogPost - Single blog post view component.
 */
function BlogPost() {
  const { slug } = useParams();
  const navigate = useNavigate();
  const { currentDealershipId } = useDealershipContext();

  const [blog, setBlog] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  /**
   * Fetches blog post by slug from API.
   */
  useEffect(() => {
    const fetchBlog = async () => {
      if (!currentDealershipId || !slug) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const response = await fetch(
          `/api/blog-posts/slug/${slug}?dealershipId=${currentDealershipId}`
        );

        if (!response.ok) {
          if (response.status === 404) {
            throw new Error('Blog post not found');
          }
          throw new Error('Failed to fetch blog post');
        }

        const result = await response.json();
        const blogData = result.data || result;
        
        // Only show published posts on public site
        if (blogData.status !== 'published') {
          throw new Error('Blog post not found');
        }

        setBlog(blogData);
      } catch (err) {
        console.error('Error fetching blog post:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchBlog();
  }, [currentDealershipId, slug]);

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
   * Handles back to blog list navigation.
   */
  const handleBackToBlog = () => {
    navigate('/blog');
  };

  // Loading state
  if (loading) {
    return (
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <p className="text-gray-600 text-center">Loading blog post...</p>
      </div>
    );
  }

  // Error state
  if (error || !blog) {
    return (
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded mb-6">
          <p className="font-medium">{error || 'Blog post not found'}</p>
        </div>
        <button
          onClick={handleBackToBlog}
          className="text-blue-600 hover:text-blue-700 font-medium"
        >
          ← Back to Blog
        </button>
      </div>
    );
  }

  return (
    <article className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Back Button */}
      <button
        onClick={handleBackToBlog}
        className="text-blue-600 hover:text-blue-700 font-medium mb-6 inline-flex items-center"
      >
        ← Back to Blog
      </button>

      {/* Featured Image */}
      {blog.featuredImageUrl && (
        <img
          src={blog.featuredImageUrl}
          alt={blog.title}
          className="w-full h-96 object-cover rounded-lg mb-8"
        />
      )}

      {/* Title */}
      <h1 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
        {blog.title}
      </h1>

      {/* Meta Information */}
      <div className="flex items-center text-gray-600 mb-8 pb-8 border-b border-gray-200">
        <div className="flex items-center">
          <div className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center text-white font-semibold mr-3">
            {blog.authorName.charAt(0).toUpperCase()}
          </div>
          <div>
            <p className="font-medium text-gray-900">{blog.authorName}</p>
            <p className="text-sm text-gray-500">
              {formatDate(blog.publishedAt)}
            </p>
          </div>
        </div>
      </div>

      {/* Excerpt */}
      {blog.excerpt && (
        <div className="text-xl text-gray-700 italic mb-8 pl-4 border-l-4 border-blue-600">
          {blog.excerpt}
        </div>
      )}

      {/* Content */}
      <div className="prose prose-lg max-w-none">
        <div className="text-gray-800 whitespace-pre-wrap leading-relaxed">
          {blog.content}
        </div>
      </div>

      {/* Footer */}
      <div className="mt-12 pt-8 border-t border-gray-200">
        <button
          onClick={handleBackToBlog}
          className="text-blue-600 hover:text-blue-700 font-medium"
        >
          ← Back to Blog
        </button>
      </div>
    </article>
  );
}

export default BlogPost;
