/**
 * @fileoverview VehicleForm - Admin vehicle create/edit form component.
 * Handles both create and edit modes with Cloudinary image uploads.
 *
 * SECURITY (SEC-001): All API calls include dealershipId parameter for multi-tenant data isolation.
 * Edit mode requires dealershipId query parameter to prevent cross-dealership modifications.
 *
 * Features:
 * - Create mode: /admin/vehicles/new (no vehicle data loaded)
 * - Edit mode: /admin/vehicles/edit/:id (pre-populates with existing vehicle data)
 * - React Hook Form for form state and validation
 * - Cloudinary upload widget for multiple image uploads (max 10)
 * - Client-side validation for all required fields
 * - Success/error messaging and navigation
 *
 * @component
 */

import { useState, useEffect, useContext } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import { AdminContext } from '../../context/AdminContext';
import Unauthorized from '../../components/Unauthorized';
import { hasPermission } from '../../utils/permissions';

/**
 * VehicleForm - Create and edit vehicle form component.
 * Renders form with all vehicle fields, image upload, and submission handlers.
 * Requires 'vehicles' permission to access.
 *
 * @returns {JSX.Element} Vehicle form component
 */
function VehicleForm() {
  const { user } = useContext(AdminContext);

  // Check permission
  if (!hasPermission(user, 'vehicles')) {
    return <Unauthorized section="Vehicle Manager" />;
  }
  const navigate = useNavigate();
  const { id } = useParams(); // Vehicle ID from URL (edit mode only)
  const { selectedDealership } = useContext(AdminContext);
  const isEditMode = Boolean(id);

  // React Hook Form setup with validation rules
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset
  } = useForm({
    defaultValues: {
      make: '',
      model: '',
      year: new Date().getFullYear(),
      price: '',
      mileage: '',
      condition: 'used',
      status: 'active',
      title: '',
      description: ''
    }
  });

  // Component state
  const [images, setImages] = useState([]); // Array of Cloudinary URLs
  const [loading, setLoading] = useState(false); // Save operation loading state
  const [fetchingVehicle, setFetchingVehicle] = useState(false); // Edit mode data fetch loading
  const [error, setError] = useState(null); // API error messages
  const [successMessage, setSuccessMessage] = useState(null); // Success feedback
  const [uploading, setUploading] = useState(false); // Image upload loading state

  /**
   * Fetches existing vehicle data for edit mode.
   * Triggered on component mount when in edit mode.
   *
   * SECURITY (SEC-001): Includes dealershipId query parameter for ownership validation.
   */
  useEffect(() => {
    const fetchVehicle = async () => {
      if (!isEditMode || !selectedDealership) return;

      try {
        setFetchingVehicle(true);
        setError(null);

        const response = await fetch(
          `/api/vehicles/${id}?dealershipId=${selectedDealership.id}`,
          { credentials: 'include' }
        );

        if (!response.ok) {
          if (response.status === 404) {
            throw new Error('Vehicle not found or does not belong to this dealership');
          }
          throw new Error('Failed to fetch vehicle data');
        }

        const vehicle = await response.json();

        // Pre-populate form fields with fetched data
        reset({
          make: vehicle.make,
          model: vehicle.model,
          year: vehicle.year,
          price: vehicle.price,
          mileage: vehicle.mileage,
          condition: vehicle.condition,
          status: vehicle.status,
          title: vehicle.title,
          description: vehicle.description || ''
        });

        // Set images array from vehicle data
        setImages(vehicle.images || []);
      } catch (err) {
        console.error('Error fetching vehicle:', err);
        setError(err.message);
      } finally {
        setFetchingVehicle(false);
      }
    };

    fetchVehicle();
  }, [id, isEditMode, selectedDealership, reset]);

  /**
   * Handles vehicle photo upload via file input.
   * Uploads multiple images to /api/upload endpoint.
   * Similar to hero background upload in DealerSettings.
   *
   * @param {Event} e - File input change event
   */
  const handlePhotoUpload = async (e) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    // Check if adding these files would exceed the limit
    if (images.length + files.length > 10) {
      setError(`Cannot upload ${files.length} files. Maximum total is 10 images (currently have ${images.length}).`);
      e.target.value = '';
      return;
    }

    // Validate file types
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/jpg'];
    const invalidFiles = files.filter(file => !allowedTypes.includes(file.type));
    if (invalidFiles.length > 0) {
      setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
      e.target.value = '';
      return;
    }

    // Validate file sizes (5MB max per file)
    const maxSize = 5 * 1024 * 1024; // 5MB in bytes
    const oversizedFiles = files.filter(file => file.size > maxSize);
    if (oversizedFiles.length > 0) {
      setError('One or more files are too large. Maximum size is 5MB per file.');
      e.target.value = '';
      return;
    }

    setUploading(true);
    setError('');

    try {
      const uploadedUrls = [];

      // Upload each file sequentially
      for (const file of files) {
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
        uploadedUrls.push(data.url);
      }

      // Add all uploaded URLs to images array
      setImages(prev => [...prev, ...uploadedUrls]);
      setSuccessMessage(`Successfully uploaded ${uploadedUrls.length} image${uploadedUrls.length > 1 ? 's' : ''}!`);

      // Auto-clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage('');
      }, 3000);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload images: ' + err.message);
    } finally {
      setUploading(false);
      e.target.value = ''; // Reset input for future uploads
    }
  };

  /**
   * Removes image at specified index from images array.
   *
   * @param {number} indexToRemove - Index of image to remove
   */
  const handleRemoveImage = (indexToRemove) => {
    setImages((prev) => prev.filter((_, index) => index !== indexToRemove));
  };

  /**
   * Handles form submission for both create and edit modes.
   * Sends POST request for create, PUT request for edit.
   *
   * SECURITY (SEC-001):
   * - Create: Includes dealership_id in request body
   * - Edit: Includes dealershipId query parameter for ownership validation
   *
   * @param {Object} formData - Form data from React Hook Form
   */
  const onSubmit = async (formData) => {
    if (!selectedDealership) {
      setError('Please select a dealership first');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccessMessage(null);

      // Prepare request payload
      const vehicleData = {
        ...formData,
        images,
        // Convert numeric strings to numbers
        year: parseInt(formData.year, 10),
        price: parseFloat(formData.price),
        mileage: parseInt(formData.mileage, 10)
      };

      let response;

      if (isEditMode) {
        // Edit mode: PUT request with dealershipId query parameter
        response = await fetch(
          `/api/vehicles/${id}?dealershipId=${selectedDealership.id}`,
          {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify(vehicleData)
          }
        );
      } else {
        // Create mode: POST request with dealership_id in body
        vehicleData.dealership_id = selectedDealership.id;
        response = await fetch('/api/vehicles', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'include',
          body: JSON.stringify(vehicleData)
        });
      }

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Failed to save vehicle');
      }

      // Success: Display message and redirect
      setSuccessMessage('Vehicle saved successfully!');

      // Wait 1 second before redirecting to allow user to see success message
      setTimeout(() => {
        navigate('/admin/vehicles');
      }, 1000);
    } catch (err) {
      console.error('Error saving vehicle:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handles cancel button click.
   * Navigates back to Vehicle Manager without saving.
   */
  const handleCancel = () => {
    navigate('/admin/vehicles');
  };

  // Guard: No dealership selected
  if (!selectedDealership) {
    return (
      <div className="container mx-auto p-4">
        <p className="text-gray-600">Please select a dealership to manage vehicles.</p>
      </div>
    );
  }

  // Loading state for edit mode data fetch
  if (fetchingVehicle) {
    return (
      <div className="container mx-auto p-4">
        <p className="text-gray-600">Loading vehicle data...</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-4">
      {/* Page Header */}
      <h1 className="text-3xl font-bold mb-6">
        {isEditMode ? 'Edit Vehicle' : 'Add New Vehicle'}
      </h1>

      {/* Success Message */}
      {successMessage && (
        <div className="bg-green-100 text-green-800 p-4 mb-4 rounded border border-green-300">
          {successMessage}
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="bg-red-100 text-red-800 p-4 mb-4 rounded border border-red-300">
          {error}
        </div>
      )}

      {/* Vehicle Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl">
        {/* Make and Model */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          <div>
            <label className="block font-semibold mb-1">
              Make <span className="text-red-600">*</span>
            </label>
            <input
              {...register('make', {
                required: 'Make is required',
                maxLength: { value: 100, message: 'Make must be 100 characters or less' }
              })}
              className="input-field w-full"
              placeholder="e.g., Toyota"
            />
            {errors.make && (
              <p className="text-red-600 text-sm mt-1">{errors.make.message}</p>
            )}
          </div>

          <div>
            <label className="block font-semibold mb-1">
              Model <span className="text-red-600">*</span>
            </label>
            <input
              {...register('model', {
                required: 'Model is required',
                maxLength: { value: 100, message: 'Model must be 100 characters or less' }
              })}
              className="input-field w-full"
              placeholder="e.g., Camry"
            />
            {errors.model && (
              <p className="text-red-600 text-sm mt-1">{errors.model.message}</p>
            )}
          </div>
        </div>

        {/* Year, Price, Mileage */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
          <div>
            <label className="block font-semibold mb-1">
              Year <span className="text-red-600">*</span>
            </label>
            <input
              type="number"
              {...register('year', {
                required: 'Year is required',
                min: { value: 1900, message: 'Year must be 1900 or later' },
                max: { value: new Date().getFullYear() + 1, message: `Year cannot exceed ${new Date().getFullYear() + 1}` },
                valueAsNumber: true
              })}
              className="input-field w-full"
              placeholder="2020"
            />
            {errors.year && (
              <p className="text-red-600 text-sm mt-1">{errors.year.message}</p>
            )}
          </div>

          <div>
            <label className="block font-semibold mb-1">
              Price <span className="text-red-600">*</span>
            </label>
            <input
              type="number"
              step="0.01"
              {...register('price', {
                required: 'Price is required',
                min: { value: 0, message: 'Price must be 0 or greater' },
                valueAsNumber: true
              })}
              className="input-field w-full"
              placeholder="25000"
            />
            {errors.price && (
              <p className="text-red-600 text-sm mt-1">{errors.price.message}</p>
            )}
          </div>

          <div>
            <label className="block font-semibold mb-1">
              Mileage <span className="text-red-600">*</span>
            </label>
            <input
              type="number"
              {...register('mileage', {
                required: 'Mileage is required',
                min: { value: 0, message: 'Mileage must be 0 or greater' },
                valueAsNumber: true
              })}
              className="input-field w-full"
              placeholder="45000"
            />
            {errors.mileage && (
              <p className="text-red-600 text-sm mt-1">{errors.mileage.message}</p>
            )}
          </div>
        </div>

        {/* Condition and Status */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          <div>
            <label className="block font-semibold mb-1">
              Condition <span className="text-red-600">*</span>
            </label>
            <select
              {...register('condition', { required: 'Condition is required' })}
              className="input-field w-full"
            >
              <option value="new">New</option>
              <option value="used">Used</option>
            </select>
            {errors.condition && (
              <p className="text-red-600 text-sm mt-1">{errors.condition.message}</p>
            )}
          </div>

          <div>
            <label className="block font-semibold mb-1">
              Status <span className="text-red-600">*</span>
            </label>
            <select
              {...register('status', { required: 'Status is required' })}
              className="input-field w-full"
            >
              <option value="active">Active</option>
              <option value="sold">Sold</option>
              <option value="pending">Pending</option>
              <option value="draft">Draft</option>
            </select>
            {errors.status && (
              <p className="text-red-600 text-sm mt-1">{errors.status.message}</p>
            )}
          </div>
        </div>

        {/* Title */}
        <div className="mb-4">
          <label className="block font-semibold mb-1">
            Title <span className="text-red-600">*</span>
          </label>
          <input
            {...register('title', {
              required: 'Title is required',
              maxLength: { value: 255, message: 'Title must be 255 characters or less' }
            })}
            className="input-field w-full"
            placeholder="e.g., 2018 Honda Civic LX"
          />
          {errors.title && (
            <p className="text-red-600 text-sm mt-1">{errors.title.message}</p>
          )}
        </div>

        {/* Description */}
        <div className="mb-6">
          <label className="block font-semibold mb-1">Description</label>
          <textarea
            {...register('description', {
              maxLength: { value: 2000, message: 'Description must be 2000 characters or less' }
            })}
            className="input-field w-full"
            rows="4"
            placeholder="Detailed description of the vehicle..."
          />
          {errors.description && (
            <p className="text-red-600 text-sm mt-1">{errors.description.message}</p>
          )}
        </div>

        {/* Image Upload Section */}
        <div className="mb-6">
          <label className="block font-semibold mb-2">Vehicle Photos</label>
          <p className="text-sm text-gray-600 mb-3">
            Upload up to 10 photos of the vehicle. Accepted formats: JPG, PNG, WebP (max 5MB per file)
          </p>
          
          {/* Upload Button */}
          <label
            className={`inline-block mb-4 ${
              images.length >= 10 || uploading
                ? 'btn-secondary cursor-not-allowed opacity-50'
                : 'btn-primary cursor-pointer'
            }`}
          >
            {uploading
              ? 'Uploading...'
              : images.length >= 10
              ? 'Maximum 10 photos reached'
              : 'Upload Photos'}
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp,image/jpg"
              multiple
              onChange={handlePhotoUpload}
              disabled={images.length >= 10 || uploading}
              className="hidden"
            />
          </label>

          {/* Image Thumbnails Grid */}
          {images.length > 0 && (
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-2">
              {images.map((url, index) => (
                <div key={index} className="relative">
                  <img
                    src={url}
                    alt={`Vehicle ${index + 1}`}
                    className="w-full h-32 object-cover rounded border border-gray-300"
                  />
                  <button
                    type="button"
                    onClick={() => handleRemoveImage(index)}
                    className="absolute top-1 right-1 bg-red-600 text-white rounded-full w-6 h-6 flex items-center justify-center hover:bg-red-700"
                    title="Remove image"
                  >
                    &times;
                  </button>
                </div>
              ))}
            </div>
          )}

          <p className="text-sm text-gray-600">
            {images.length} / 10 photos uploaded
          </p>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-4">
          <button
            type="submit"
            disabled={loading}
            className={`btn-primary ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
          >
            {loading ? 'Saving...' : 'Save Vehicle'}
          </button>
          <button
            type="button"
            onClick={handleCancel}
            disabled={loading}
            className="btn-secondary"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}

export default VehicleForm;
