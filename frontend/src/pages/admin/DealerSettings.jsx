/**
 * DealerSettings - Admin page for editing dealership profile information.
 * 
 * @component
 * @description Allows dealership staff to update name, logo, contact details,
 * hours, and about text. Changes immediately reflect on public website.
 * 
 * Features:
 * - Fetches and pre-populates existing dealership data
 * - Cloudinary logo upload with cropping
 * - Form validation (required fields, email format)
 * - Success/error feedback
 * - Mobile-responsive layout
 * 
 * @example
 * <Route path="/admin/settings" element={<ProtectedRoute><DealerSettings /></ProtectedRoute>} />
 */

import { useState, useEffect, useContext } from 'react';
import { useForm } from 'react-hook-form';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';
import NavigationManager from '../../components/admin/NavigationManager';
import TemplateSelector from '../../components/admin/TemplateSelector';
import { EasyCarsSettings } from '../../components/admin/EasyCarsSettings';
import Unauthorized from '../../components/Unauthorized';
import { hasPermission } from '../../utils/permissions';
import apiRequest from '../../utils/api';

function DealerSettings() {
  const { selectedDealership, setSelectedDealership, user } = useContext(AdminContext);

  // Check if user can edit settings
  const canEditSettings = hasPermission(user, 'settings');

  const [logoUrl, setLogoUrl] = useState('');
  const [heroBackgroundUrl, setHeroBackgroundUrl] = useState('');
  const [heroType, setHeroType] = useState('image');
  const [heroVideoUrl, setHeroVideoUrl] = useState('');
  const [heroCarouselImages, setHeroCarouselImages] = useState([]);
  const [heroTitle, setHeroTitle] = useState('');
  const [heroSubtitle, setHeroSubtitle] = useState('');
  const [themeColor, setThemeColor] = useState('#3B82F6');
  const [secondaryThemeColor, setSecondaryThemeColor] = useState('#FFFFFF');
  const [bodyBackgroundColor, setBodyBackgroundColor] = useState('#FFFFFF');
  const [fontFamily, setFontFamily] = useState('system');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [financePolicyCount, setFinancePolicyCount] = useState(0);
  const [warrantyPolicyCount, setWarrantyPolicyCount] = useState(0);
  const [facebookUrl, setFacebookUrl] = useState('');
  const [instagramUrl, setInstagramUrl] = useState('');
  const [financePromoImage, setFinancePromoImage] = useState('');
  const [financePromoText, setFinancePromoText] = useState('');
  const [warrantyPromoImage, setWarrantyPromoImage] = useState('');
  const [warrantyPromoText, setWarrantyPromoText] = useState('');
  const [websiteUrl, setWebsiteUrl] = useState('');

  const {
    register,
    handleSubmit: formHandleSubmit,
    reset,
    formState: { errors }
  } = useForm({
    defaultValues: {
      name: '',
      address: '',
      phone: '',
      email: '',
      hours: '',
      financePolicy: '',
      warrantyPolicy: '',
      about: ''
    }
  });

  /**
   * Fetches existing dealership data on component mount or when selectedDealership changes.
   * Pre-populates form fields with fetched data.
   */
  useEffect(() => {
    if (selectedDealership) {
      const fetchDealershipData = async () => {
        try {
          const response = await apiRequest(`/api/dealers/${selectedDealership.id}`);
          if (response.ok) {
            const apiResponse = await response.json();
            const data = apiResponse.data || apiResponse;
            
            reset({
              name: data.name || '',
              address: data.address || '',
              phone: data.phone || '',
              email: data.email || '',
              hours: data.hours || '',
              financePolicy: data.financePolicy || '',
              warrantyPolicy: data.warrantyPolicy || '',
              about: data.about || ''
            });
            setLogoUrl(data.logoUrl || '');
            setHeroBackgroundUrl(data.heroBackgroundImage || '');
            setHeroType(data.heroType || 'image');
            setHeroVideoUrl(data.heroVideoUrl || '');
            setHeroCarouselImages(data.heroCarouselImages || []);
            setHeroTitle(data.heroTitle || '');
            setHeroSubtitle(data.heroSubtitle || '');
            setThemeColor(data.themeColor || '#3B82F6');
            setSecondaryThemeColor(data.secondaryThemeColor || '#FFFFFF');
            setBodyBackgroundColor(data.bodyBackgroundColor || '#FFFFFF');
            setFontFamily(data.fontFamily || 'system');
            setFinancePolicyCount((data.financePolicy || '')?.length || 0);
            setWarrantyPolicyCount((data.warrantyPolicy || '')?.length || 0);
            setFacebookUrl(data.facebookUrl || '');
            setInstagramUrl(data.instagramUrl || '');
            setFinancePromoImage(data.financePromoImage || '');
            setFinancePromoText(data.financePromoText || '');
            setWarrantyPromoImage(data.warrantyPromoImage || '');
            setWarrantyPromoText(data.warrantyPromoText || '');
            setWebsiteUrl(data.websiteUrl || '');
          }
        } catch (err) {
          console.error('Failed to fetch dealership data:', err);
          setError('Failed to load dealership data. Please refresh the page.');
        }
      };
      fetchDealershipData();
    }
  }, [selectedDealership, reset]);

  /**
   * Handles logo upload via Cloudinary upload widget.
   * 
   * Opens Cloudinary widget configured for single logo upload with square cropping.
   * Updates logoUrl state with returned Cloudinary URL on success.
   * 
   * @function handleLogoUpload
   * @returns {void}
   */
  const handleLogoUpload = () => {
    if (window.cloudinary) {
      window.cloudinary.openUploadWidget(
        {
          cloudName: import.meta.env.VITE_CLOUDINARY_CLOUD_NAME,
          uploadPreset: import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET,
          sources: ['local'],
          multiple: false,
          maxFileSize: 2000000, // 2MB
          clientAllowedFormats: ['jpg', 'png', 'svg'],
          cropping: true,
          croppingAspectRatio: 1, // Square crop
          folder: 'dealership-logos',
          resourceType: 'image'
        },
        (error, result) => {
          if (error) {
            console.error('Cloudinary upload error:', error);
            setError('Failed to upload logo. Please try again.');
            return;
          }
          if (result.event === 'success') {
            setLogoUrl(result.info.secure_url);
          }
        }
      );
    } else {
      console.error('Cloudinary widget not loaded');
      setError('Upload widget is not available. Please refresh the page.');
      // Fallback: If Cloudinary widget fails, fallback to `/api/upload` endpoint
    }
  };

  /**
   * Handles logo removal.
   * Clears logoUrl state, allowing user to upload a new logo.
   *
   * @function handleRemoveLogo
   * @returns {void}
   */
  const handleRemoveLogo = () => {
    setLogoUrl('');
  };

  /**
   * Handles hero background image upload via Cloudinary upload widget.
   * 
   * Opens Cloudinary widget configured for hero background image upload.
   * Updates heroBackgroundUrl state with returned Cloudinary URL on success.
   * 
   * @function handleHeroBackgroundUpload
   * @returns {void}
   */
  const handleHeroBackgroundUpload = () => {
    if (window.cloudinary) {
      window.cloudinary.openUploadWidget(
        {
          cloudName: import.meta.env.VITE_CLOUDINARY_CLOUD_NAME,
          uploadPreset: import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET,
          sources: ['local'],
          multiple: false,
          maxFileSize: 5000000, // 5MB
          clientAllowedFormats: ['jpg', 'png', 'webp'],
          folder: 'dealership-heroes',
          resourceType: 'image'
        },
        (error, result) => {
          if (error) {
            console.error('Cloudinary upload error:', error);
            setError('Failed to upload hero background. Please try again.');
            return;
          }
          if (result.event === 'success') {
            setHeroBackgroundUrl(result.info.secure_url);
            setSuccessMessage('Hero background image uploaded successfully!');
            setTimeout(() => setSuccessMessage(''), 5000);
          }
        }
      );
    } else {
      console.error('Cloudinary widget not loaded');
      setError('Upload widget is not available. Please refresh the page.');
    }
  };

  /**
   * Handles hero background image removal.
   * Clears heroBackgroundUrl state, allowing user to upload a new image.
   *
   * @function handleRemoveHeroBackground
   * @returns {void}
   */
  const handleRemoveHeroBackground = () => {
    setHeroBackgroundUrl('');
  };

  /**
   * Handles hero video file selection and upload.
   * Uses file input and /api/upload endpoint for video uploads.
   *
   * @function handleHeroVideoUpload
   * @param {Event} e - File input change event
   * @returns {Promise<void>}
   */
  const handleHeroVideoUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['video/mp4', 'video/webm', 'video/ogg'];
    if (!allowedTypes.includes(file.type)) {
      setError('Invalid file type. Please upload MP4, WebM, or OGG videos only.');
      e.target.value = '';
      return;
    }

    // Validate file size (50MB max for videos)
    const maxSize = 50 * 1024 * 1024; // 50MB in bytes
    if (file.size > maxSize) {
      setError('File too large. Maximum size is 50MB.');
      e.target.value = '';
      return;
    }

    setLoading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', file); // Using same field name for consistency

      const response = await apiRequest('/api/upload', {
        method: 'POST',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Upload failed');
      }

      const data = await response.json();
      setHeroVideoUrl(data.url);
      setSuccessMessage('Hero video uploaded successfully!');

      setTimeout(() => {
        setSuccessMessage('');
      }, 5000);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload video: ' + err.message);
    } finally {
      setLoading(false);
      e.target.value = '';
    }
  };

  /**
   * Handles hero video removal.
   *
   * @function handleRemoveHeroVideo
   * @returns {void}
   */
  const handleRemoveHeroVideo = () => {
    setHeroVideoUrl('');
  };

  /**
   * Handles carousel image upload.
   * Adds new image to carousel images array.
   *
   * @function handleCarouselImageUpload
   * @param {Event} e - File input change event
   * @returns {Promise<void>}
   */
  const handleCarouselImageUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
      e.target.value = '';
      return;
    }

    // Validate file size (5MB max)
    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      setError('File too large. Maximum size is 5MB.');
      e.target.value = '';
      return;
    }

    setLoading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', file);

      const response = await apiRequest('/api/upload', {
        method: 'POST',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Upload failed');
      }

      const data = await response.json();
      setHeroCarouselImages([...heroCarouselImages, data.url]);
      setSuccessMessage('Carousel image added successfully!');

      setTimeout(() => {
        setSuccessMessage('');
      }, 5000);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload image: ' + err.message);
    } finally {
      setLoading(false);
      e.target.value = '';
    }
  };

  /**
   * Removes a carousel image by index.
   *
   * @function handleRemoveCarouselImage
   * @param {number} index - Index of image to remove
   * @returns {void}
   */
  const handleRemoveCarouselImage = (index) => {
    setHeroCarouselImages(heroCarouselImages.filter((_, i) => i !== index));
  };

  /**
   * Refreshes dealership data from API.
   * Used after successful navigation config save.
   *
   * @function refreshDealership
   * @returns {Promise<void>}
   */
  const refreshDealership = async () => {
    if (selectedDealership) {
      try {
        const response = await apiRequest(`/api/dealers/${selectedDealership.id}`);
        if (response.ok) {
          const apiResponse = await response.json();
          const updatedData = apiResponse.data || apiResponse;
          setSelectedDealership(updatedData);
        }
      } catch (err) {
        console.error('Failed to refresh dealership data:', err);
      }
    }
  };

  /**
   * Handles finance promotional panel image upload.
   * Uses file input and /api/upload endpoint.
   *
   * @function handleFinancePromoImageUpload
   * @param {Event} e - File input change event
   * @returns {Promise<void>}
   */
  const handleFinancePromoImageUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
      e.target.value = '';
      return;
    }

    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      setError('File too large. Maximum size is 5MB.');
      e.target.value = '';
      return;
    }

    setLoading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', file);

      const response = await apiRequest('/api/upload', {
        method: 'POST',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Upload failed');
      }

      const data = await response.json();
      setFinancePromoImage(data.url);
      setSuccessMessage('Finance promo image uploaded successfully!');

      setTimeout(() => {
        setSuccessMessage('');
      }, 5000);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload image: ' + err.message);
    } finally {
      setLoading(false);
      e.target.value = '';
    }
  };

  /**
   * Handles warranty promotional panel image upload.
   * Uses file input and /api/upload endpoint.
   *
   * @function handleWarrantyPromoImageUpload
   * @param {Event} e - File input change event
   * @returns {Promise<void>}
   */
  const handleWarrantyPromoImageUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
      e.target.value = '';
      return;
    }

    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      setError('File too large. Maximum size is 5MB.');
      e.target.value = '';
      return;
    }

    setLoading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', file);

      const response = await apiRequest('/api/upload', {
        method: 'POST',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Upload failed');
      }

      const data = await response.json();
      setWarrantyPromoImage(data.url);
      setSuccessMessage('Warranty promo image uploaded successfully!');

      setTimeout(() => {
        setSuccessMessage('');
      }, 5000);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload image: ' + err.message);
    } finally {
      setLoading(false);
      e.target.value = '';
    }
  };

  /**
   * Handles form submission.
   * Sends PUT request to update dealership data.
   * Updates AdminContext with new data on success.
   *
   * @function handleSubmit
   * @param {Object} formData - Form data from React Hook Form
   * @returns {Promise<void>}
   */
  const handleSubmit = async (formData) => {
    setLoading(true);
    setError('');
    setSuccessMessage('');

    const dealershipData = {
      name: formData.name,
      logoUrl: logoUrl || null,
      address: formData.address,
      phone: formData.phone,
      email: formData.email,
      hours: formData.hours || null,
      financePolicy: formData.financePolicy || null,
      warrantyPolicy: formData.warrantyPolicy || null,
      about: formData.about || null,
      heroBackgroundImage: heroBackgroundUrl || null,
      heroType: heroType,
      heroVideoUrl: heroVideoUrl || null,
      heroCarouselImages: heroCarouselImages,
      heroTitle: heroTitle || null,
      heroSubtitle: heroSubtitle || null,
      themeColor: themeColor,
      secondaryThemeColor: secondaryThemeColor,
      bodyBackgroundColor: bodyBackgroundColor,
      fontFamily: fontFamily,
      facebookUrl: facebookUrl || null,
      instagramUrl: instagramUrl || null,
      financePromoImage: financePromoImage || null,
      financePromoText: financePromoText || null,
      warrantyPromoImage: warrantyPromoImage || null,
      warrantyPromoText: warrantyPromoText || null,
      websiteUrl: websiteUrl || null
    };

    try {
      const response = await apiRequest(`/api/dealers/${selectedDealership.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(dealershipData)
      });

      if (response.ok) {
        const apiResponse = await response.json();
        const updatedData = apiResponse.data || apiResponse;
        setSelectedDealership(updatedData);
        setSuccessMessage('Dealership settings updated successfully!');
        
        // Auto-clear success message after 5 seconds
        setTimeout(() => {
          setSuccessMessage('');
        }, 5000);
      } else {
        const errorData = await response.json();
        setError(errorData.message || errorData.error || 'Failed to update dealership settings.');
      }
    } catch (err) {
      console.error('Failed to update dealership:', err);
      setError('Network error. Please check your connection and try again.');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handles template application.
   * Updates the current theme settings with template values.
   *
   * @param {Object} template - Template settings to apply
   */
  const handleApplyTemplate = (template) => {
    setThemeColor(template.themeColor);
    setSecondaryThemeColor(template.secondaryThemeColor);
    setBodyBackgroundColor(template.bodyBackgroundColor);
    setFontFamily(template.fontFamily);
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <AdminHeader />

      <div className="p-4">
        {/* Template Selector - Full Width */}
        <div className="max-w-7xl mx-auto mb-6">
          {canEditSettings && (
            <TemplateSelector
              currentSettings={{
                themeColor: themeColor,
                secondaryThemeColor: secondaryThemeColor,
                bodyBackgroundColor: bodyBackgroundColor,
                fontFamily: fontFamily
              }}
              onApplyTemplate={handleApplyTemplate}
            />
          )}
        </div>

        {/* Basic Settings - Narrower Container */}
        <div className="max-w-3xl mx-auto">
        {/* Read-only banner for users without edit permission */}
        {!canEditSettings && (
          <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6">
            <div className="flex">
              <div className="flex-shrink-0">
                <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                </svg>
              </div>
              <div className="ml-3">
                <p className="text-sm text-yellow-700">
                  <strong>View Only:</strong> You can view all dealership settings but cannot modify them. Contact your dealership owner to request settings management permission.
                </p>
              </div>
            </div>
          </div>
        )}

        <div className="card bg-white p-6 shadow-md rounded-lg">
          <h1 className="text-2xl font-bold mb-6">Dealership Settings</h1>

          {/* Success Message */}
          {successMessage && (
            <div className="mb-4 p-3 bg-green-100 text-green-800 rounded">
              {successMessage}
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="mb-4 p-3 bg-red-100 text-red-800 rounded">
              {error}
            </div>
          )}

          <form onSubmit={formHandleSubmit(handleSubmit)} className="space-y-4">
            {/* Dealership Name */}
            <div>
              <label htmlFor="name" className="block font-medium mb-1">
                Dealership Name <span className="text-red-500">*</span>
              </label>
              <input
                id="name"
                type="text"
                className="input-field w-full"
                disabled={loading || !canEditSettings}
                {...register('name', {
                  required: 'Dealership name is required',
                  maxLength: {
                    value: 255,
                    message: 'Name must be 255 characters or less'
                  }
                })}
              />
              {errors.name && (
                <p className="text-red-500 text-sm mt-1">{errors.name.message}</p>
              )}
            </div>

            {/* Website URL */}
            <div>
              <label htmlFor="websiteUrl" className="block font-medium mb-1">
                Website URL
              </label>
              <p className="text-sm text-gray-600 mb-2">
                Custom URL/domain for this dealership's website (e.g., acme-auto.com). Leave empty if not applicable.
              </p>
              <input
                id="websiteUrl"
                type="text"
                placeholder="e.g., acme-auto.com"
                value={websiteUrl}
                onChange={(e) => setWebsiteUrl(e.target.value)}
                disabled={loading || !canEditSettings}
                className="input-field w-full"
                maxLength={255}
              />
              <p className="text-xs text-gray-500 mt-1">
                This URL will be used to identify your dealership's website. It must be unique across all dealerships.
              </p>
            </div>

            {/* Theme Color Picker */}
            <div>
              <label htmlFor="theme_color" className="block font-medium mb-1">
                Primary Theme Color
              </label>
              <p className="text-sm text-gray-600 mb-2">
                Choose a primary color for your dealership's branding. This will be used for the website header background and other elements.
              </p>
              <div className="flex items-center gap-4">
                <input
                  id="theme_color"
                  type="color"
                  value={themeColor}
                  onChange={(e) => setThemeColor(e.target.value)}
                  disabled={loading || !canEditSettings}
                  className="h-12 w-20 rounded border border-gray-300 cursor-pointer disabled:opacity-50"
                />
                <div className="flex-1">
                  <input
                    type="text"
                    value={themeColor}
                    onChange={(e) => setThemeColor(e.target.value)}
                    disabled={loading}
                    placeholder="#3B82F6"
                    pattern="^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"
                    className="input-field w-32"
                  />
                  <p className="text-xs text-gray-500 mt-1">Hex color code (e.g., #3B82F6)</p>
                </div>
                <button
                  type="button"
                  onClick={() => setThemeColor('#3B82F6')}
                  disabled={loading}
                  className="btn-secondary text-sm"
                >
                  Reset to Default
                </button>
              </div>
              {/* Preview */}
              <div className="mt-3 p-4 rounded" style={{ backgroundColor: themeColor }}>
                <p className="text-white font-semibold drop-shadow">Header Preview</p>
              </div>
            </div>

            {/* Secondary Theme Color Picker */}
            <div>
              <label htmlFor="secondary_theme_color" className="block font-medium mb-1">
                Secondary Theme Color
              </label>
              <p className="text-sm text-gray-600 mb-2">
                Choose a secondary color for your dealership's branding. This will be used for buttons, accents, and complementary elements.
              </p>
              <div className="flex items-center gap-4">
                <input
                  id="secondary_theme_color"
                  type="color"
                  value={secondaryThemeColor}
                  onChange={(e) => setSecondaryThemeColor(e.target.value)}
                  disabled={loading}
                  className="h-12 w-20 rounded border border-gray-300 cursor-pointer disabled:opacity-50"
                />
                <div className="flex-1">
                  <input
                    type="text"
                    value={secondaryThemeColor}
                    onChange={(e) => setSecondaryThemeColor(e.target.value)}
                    disabled={loading}
                    placeholder="#FFFFFF"
                    pattern="^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"
                    className="input-field w-32"
                  />
                  <p className="text-xs text-gray-500 mt-1">Hex color code (e.g., #FFFFFF)</p>
                </div>
                <button
                  type="button"
                  onClick={() => setSecondaryThemeColor('#FFFFFF')}
                  disabled={loading}
                  className="btn-secondary text-sm"
                >
                  Reset to Default
                </button>
              </div>
              {/* Preview */}
              <div className="mt-3 flex gap-2">
                <button
                  type="button"
                  className="px-4 py-2 rounded text-white font-semibold"
                  style={{ backgroundColor: secondaryThemeColor }}
                >
                  Button Preview
                </button>
                <div className="flex-1 p-4 border-2 rounded" style={{ borderColor: secondaryThemeColor }}>
                  <p className="text-sm" style={{ color: secondaryThemeColor }}>Accent Text Preview</p>
                </div>
              </div>
            </div>

            {/* Body Background Color Picker */}
            <div>
              <label htmlFor="body_background_color" className="block font-medium mb-1">
                Body Background Color
              </label>
              <p className="text-sm text-gray-600 mb-2">
                Choose a background color for the main body of your dealership website.
              </p>
              <div className="flex items-center gap-4">
                <input
                  id="body_background_color"
                  type="color"
                  value={bodyBackgroundColor}
                  onChange={(e) => setBodyBackgroundColor(e.target.value)}
                  disabled={loading}
                  className="h-12 w-20 rounded border border-gray-300 cursor-pointer disabled:opacity-50"
                />
                <div className="flex-1">
                  <input
                    type="text"
                    value={bodyBackgroundColor}
                    onChange={(e) => setBodyBackgroundColor(e.target.value)}
                    disabled={loading}
                    placeholder="#FFFFFF"
                    pattern="^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"
                    className="input-field w-32"
                  />
                  <p className="text-xs text-gray-500 mt-1">Hex color code (e.g., #FFFFFF)</p>
                </div>
                <button
                  type="button"
                  onClick={() => setBodyBackgroundColor('#FFFFFF')}
                  disabled={loading}
                  className="btn-secondary text-sm"
                >
                  Reset to Default
                </button>
              </div>
              {/* Preview */}
              <div className="mt-3 p-8 rounded border-2 border-gray-300" style={{ backgroundColor: bodyBackgroundColor }}>
                <p className="text-gray-800 font-semibold mb-2">Body Background Preview</p>
                <p className="text-gray-600 text-sm">This is how the website background will look with content.</p>
              </div>
            </div>

            {/* Font Family Selector */}
            <div>
              <label htmlFor="font_family" className="block font-medium mb-1">
                Website Font
              </label>
              <p className="text-sm text-gray-600 mb-2">
                Choose a font for all text on your dealership website.
              </p>
              <select
                id="font_family"
                value={fontFamily}
                onChange={(e) => setFontFamily(e.target.value)}
                disabled={loading}
                className="input-field w-full"
              >
                <option value="system">System Default (Sans Serif)</option>
                <option value="arial">Arial</option>
                <option value="times">Times New Roman</option>
                <option value="georgia">Georgia</option>
                <option value="verdana">Verdana</option>
                <option value="courier">Courier New</option>
                <option value="comic-sans">Comic Sans MS</option>
                <option value="trebuchet">Trebuchet MS</option>
                <option value="impact">Impact</option>
                <option value="palatino">Palatino</option>
              </select>
              {/* Preview */}
              <div className="mt-3 p-4 border border-gray-300 rounded" style={{ 
                fontFamily: fontFamily === 'system' ? '-apple-system, BlinkMacSystemFont, "Segoe UI", "Roboto", "Oxygen", "Ubuntu", "Cantarell", "Fira Sans", "Droid Sans", "Helvetica Neue", sans-serif' :
                           fontFamily === 'arial' ? 'Arial, Helvetica, sans-serif' :
                           fontFamily === 'times' ? '"Times New Roman", Times, serif' :
                           fontFamily === 'georgia' ? 'Georgia, serif' :
                           fontFamily === 'verdana' ? 'Verdana, Geneva, sans-serif' :
                           fontFamily === 'courier' ? '"Courier New", Courier, monospace' :
                           fontFamily === 'comic-sans' ? '"Comic Sans MS", cursive, sans-serif' :
                           fontFamily === 'trebuchet' ? '"Trebuchet MS", Helvetica, sans-serif' :
                           fontFamily === 'impact' ? 'Impact, Charcoal, sans-serif' :
                           fontFamily === 'palatino' ? '"Palatino Linotype", "Book Antiqua", Palatino, serif' : 'inherit'
              }}>
                <p className="text-lg mb-2">Font Preview: The quick brown fox jumps over the lazy dog.</p>
                <p className="text-sm text-gray-600">This is how your text will appear on the website.</p>
              </div>
            </div>

            {/* Logo Upload Section */}
            <div>
              <label className="block font-medium mb-1">Dealership Logo</label>
              {logoUrl ? (
                <div className="flex items-center gap-4">
                  <img
                    src={logoUrl.replace('/upload/', '/upload/w_200,h_200,c_fit,f_auto/')}
                    alt="Dealership Logo"
                    className="w-24 h-24 object-contain border border-gray-300 rounded"
                  />
                  <button
                    type="button"
                    onClick={handleRemoveLogo}
                    disabled={loading}
                    className="btn-danger"
                  >
                    Remove Logo
                  </button>
                </div>
              ) : (
                <button
                  type="button"
                  onClick={handleLogoUpload}
                  disabled={loading}
                  className="btn-secondary"
                >
                  Upload Logo
                </button>
              )}
            </div>

            {/* Hero Media Section */}
            <div className="border-t pt-4 mt-4">
              <h2 className="text-xl font-bold mb-4">Home Page Hero Section</h2>
              
              {/* Hero Type Selector */}
              <div className="mb-4">
                <label className="block font-medium mb-2">Hero Background Type</label>
                <p className="text-sm text-gray-600 mb-3">
                  Choose how you want your home page hero section to appear.
                </p>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <label className={`p-4 border-2 rounded-lg cursor-pointer transition ${
                    heroType === 'image' ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-blue-300'
                  }`}>
                    <input
                      type="radio"
                      name="heroType"
                      value="image"
                      checked={heroType === 'image'}
                      onChange={(e) => setHeroType(e.target.value)}
                      disabled={loading}
                      className="mr-2"
                    />
                    <span className="font-medium">Single Image</span>
                    <p className="text-sm text-gray-600 mt-1">One static background image</p>
                  </label>
                  
                  <label className={`p-4 border-2 rounded-lg cursor-pointer transition ${
                    heroType === 'video' ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-blue-300'
                  }`}>
                    <input
                      type="radio"
                      name="heroType"
                      value="video"
                      checked={heroType === 'video'}
                      onChange={(e) => setHeroType(e.target.value)}
                      disabled={loading}
                      className="mr-2"
                    />
                    <span className="font-medium">Video</span>
                    <p className="text-sm text-gray-600 mt-1">Looping background video</p>
                  </label>
                  
                  <label className={`p-4 border-2 rounded-lg cursor-pointer transition ${
                    heroType === 'carousel' ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-blue-300'
                  }`}>
                    <input
                      type="radio"
                      name="heroType"
                      value="carousel"
                      checked={heroType === 'carousel'}
                      onChange={(e) => setHeroType(e.target.value)}
                      disabled={loading}
                      className="mr-2"
                    />
                    <span className="font-medium">Image Carousel</span>
                    <p className="text-sm text-gray-600 mt-1">Rotating slideshow of images</p>
                  </label>
                </div>
              </div>

              {/* Single Image Upload */}
              {heroType === 'image' && (
                <div className="mt-4">
                  <label className="block font-medium mb-1">
                    Hero Background Image
                  </label>
                  <p className="text-sm text-gray-600 mb-2">
                    Upload a custom background image for your dealership's home page hero section.
                  </p>
                  {heroBackgroundUrl ? (
                    <div className="space-y-3">
                      <div className="relative w-full h-48 border border-gray-300 rounded overflow-hidden">
                        <img
                          src={heroBackgroundUrl.replace('/upload/', '/upload/w_800,h_400,c_fill,f_auto/')}
                          alt="Hero Background Preview"
                          className="w-full h-full object-cover"
                        />
                        <div className="absolute inset-0 bg-black bg-opacity-40 flex items-center justify-center">
                          <p className="text-white text-lg font-semibold drop-shadow-lg">Preview with overlay</p>
                        </div>
                      </div>
                      <div className="flex gap-3">
                        <button type="button" onClick={handleHeroBackgroundUpload} disabled={loading} className="btn-secondary">Change Image</button>
                        <button
                          type="button"
                          onClick={handleRemoveHeroBackground}
                          disabled={loading}
                          className="btn-danger"
                        >
                          Remove Image
                        </button>
                      </div>
                    </div>
                  ) : (
                    <button type="button" onClick={handleHeroBackgroundUpload} disabled={loading} className="btn-secondary">Upload Hero Background Image</button>
                  )}
                </div>
              )}

              {/* Video Upload */}
              {heroType === 'video' && (
                <div className="mt-4">
                  <label className="block font-medium mb-1">
                    Hero Background Video
                  </label>
                  <p className="text-sm text-gray-600 mb-2">
                    Upload a video to use as your hero background. Video will loop automatically. (Max 50MB)
                  </p>
                  {heroVideoUrl ? (
                    <div className="space-y-3">
                      <div className="relative w-full h-48 border border-gray-300 rounded overflow-hidden">
                        <video
                          src={heroVideoUrl}
                          className="w-full h-full object-cover"
                          autoPlay
                          loop
                          muted
                          playsInline
                        />
                        <div className="absolute inset-0 bg-black bg-opacity-40 flex items-center justify-center">
                          <p className="text-white text-lg font-semibold drop-shadow-lg">Preview with overlay</p>
                        </div>
                      </div>
                      <div className="flex gap-3">
                        <label className="btn-secondary cursor-pointer">
                          Change Video
                          <input
                            type="file"
                            accept="video/mp4,video/webm,video/ogg"
                            onChange={handleHeroVideoUpload}
                            disabled={loading}
                            className="hidden"
                          />
                        </label>
                        <button
                          type="button"
                          onClick={handleRemoveHeroVideo}
                          disabled={loading}
                          className="btn-danger"
                        >
                          Remove Video
                        </button>
                      </div>
                    </div>
                  ) : (
                    <label className="btn-secondary cursor-pointer inline-block">
                      Upload Hero Background Video
                      <input
                        type="file"
                        accept="video/mp4,video/webm,video/ogg"
                        onChange={handleHeroVideoUpload}
                        disabled={loading}
                        className="hidden"
                      />
                    </label>
                  )}
                </div>
              )}

              {/* Carousel Upload */}
              {heroType === 'carousel' && (
                <div className="mt-4">
                  <label className="block font-medium mb-1">
                    Hero Carousel Images
                  </label>
                  <p className="text-sm text-gray-600 mb-2">
                    Upload multiple images to create a rotating carousel. Images will automatically transition every 5 seconds.
                  </p>
                  
                  {/* Carousel Images Grid */}
                  {heroCarouselImages.length > 0 && (
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-4">
                      {heroCarouselImages.map((imageUrl, index) => (
                        <div key={index} className="relative group">
                          <div className="relative w-full h-32 border border-gray-300 rounded overflow-hidden">
                            <img
                              src={imageUrl.replace('/upload/', '/upload/w_400,h_250,c_fill,f_auto/')}
                              alt={`Carousel ${index + 1}`}
                              className="w-full h-full object-cover"
                            />
                            <div className="absolute top-2 left-2 bg-black bg-opacity-60 text-white px-2 py-1 rounded text-sm">
                              #{index + 1}
                            </div>
                          </div>
                          <button
                            type="button"
                            onClick={() => handleRemoveCarouselImage(index)}
                            disabled={loading}
                            className="absolute top-2 right-2 bg-red-500 text-white p-1 rounded-full hover:bg-red-600 transition opacity-0 group-hover:opacity-100"
                            title="Remove image"
                          >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                  
                  {/* Add Image Button */}
                  <label className="btn-secondary cursor-pointer inline-block">
                    {heroCarouselImages.length > 0 ? 'Add Another Image' : 'Add First Image'}
                    <input
                      type="file"
                      accept="image/jpeg,image/png,image/webp"
                      onChange={handleCarouselImageUpload}
                      disabled={loading}
                      className="hidden"
                    />
                  </label>
                  
                  {heroCarouselImages.length > 0 && (
                    <p className="text-sm text-gray-500 mt-2">
                      {heroCarouselImages.length} image{heroCarouselImages.length !== 1 ? 's' : ''} in carousel
                    </p>
                  )}
                </div>
              )}
            </div>

            {/* Hero Text Content */}
            <div className="border-t pt-4 mt-4">
              <h2 className="text-xl font-bold mb-4">Hero Text Content</h2>
              <p className="text-sm text-gray-600 mb-4">
                Customize the text displayed on your hero section. Leave blank to hide the text.
              </p>
              
              {/* Hero Title */}
              <div className="mb-4">
                <label htmlFor="heroTitle" className="block font-medium mb-1">
                  Hero Title (Optional)
                </label>
                <p className="text-sm text-gray-600 mb-2">
                  Leave blank to show no title on the hero section
                </p>
                <input
                  id="heroTitle"
                  type="text"
                  className="input-field w-full"
                  value={heroTitle}
                  onChange={(e) => setHeroTitle(e.target.value)}
                  disabled={loading || !canEditSettings}
                  placeholder="e.g., Hot Spot Autos"
                  maxLength={100}
                />
              </div>

              {/* Hero Subtitle */}
              <div>
                <label htmlFor="heroSubtitle" className="block font-medium mb-1">
                  Hero Subtitle (Optional)
                </label>
                <p className="text-sm text-gray-600 mb-2">
                  Leave blank to show no subtitle on the hero section
                </p>
                <textarea
                  id="heroSubtitle"
                  rows="3"
                  className="input-field w-full"
                  value={heroSubtitle}
                  onChange={(e) => setHeroSubtitle(e.target.value)}
                  disabled={loading || !canEditSettings}
                  placeholder="e.g., Quality vehicles at great prices. Browse our inventory to find your next car."
                  maxLength={300}
                />
              </div>
            </div>

            {/* Address */}
            <div>
              <label htmlFor="address" className="block font-medium mb-1">
                Address <span className="text-red-500">*</span>
              </label>
              <textarea
                id="address"
                rows="3"
                className="input-field w-full"
                disabled={loading}
                {...register('address', {
                  required: 'Address is required'
                })}
              />
              {errors.address && (
                <p className="text-red-500 text-sm mt-1">{errors.address.message}</p>
              )}
            </div>

            {/* Phone */}
            <div>
              <label htmlFor="phone" className="block font-medium mb-1">
                Phone <span className="text-red-500">*</span>
              </label>
              <input
                id="phone"
                type="text"
                className="input-field w-full"
                disabled={loading}
                {...register('phone', {
                  required: 'Phone number is required',
                  maxLength: {
                    value: 20,
                    message: 'Phone must be 20 characters or less'
                  }
                })}
              />
              {errors.phone && (
                <p className="text-red-500 text-sm mt-1">{errors.phone.message}</p>
              )}
            </div>

            {/* Email */}
            <div>
              <label htmlFor="email" className="block font-medium mb-1">
                Email <span className="text-red-500">*</span>
              </label>
              <input
                id="email"
                type="email"
                className="input-field w-full"
                disabled={loading}
                {...register('email', {
                  required: 'Email is required',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: 'Invalid email format'
                  }
                })}
              />
              {errors.email && (
                <p className="text-red-500 text-sm mt-1">{errors.email.message}</p>
              )}
            </div>

            {/* Hours */}
            <div>
              <label htmlFor="hours" className="block font-medium mb-1">
                Business Hours
              </label>
              <textarea
                id="hours"
                rows="3"
                placeholder="e.g., Mon-Fri 9am-6pm, Sat 10am-4pm"
                className="input-field w-full"
                disabled={loading}
                {...register('hours', {
                  maxLength: {
                    value: 500,
                    message: 'Hours must be 500 characters or less'
                  }
                })}
              />
              {errors.hours && (
                <p className="text-red-500 text-sm mt-1">{errors.hours.message}</p>
              )}
            </div>

            {/* Social Media Links Section */}
            <div className="border-t pt-6">
              <h2 className="text-xl font-semibold mb-4">Social Media Links</h2>
              <p className="text-sm text-gray-600 mb-4">
                Add your dealership's social media profile URLs. These will be displayed in the website footer.
              </p>

              {/* Facebook URL */}
              <div className="mb-4">
                <label htmlFor="facebook_url" className="block font-medium mb-1">
                  Facebook Page URL
                </label>
                <input
                  id="facebook_url"
                  type="url"
                  value={facebookUrl}
                  onChange={(e) => setFacebookUrl(e.target.value)}
                  placeholder="https://www.facebook.com/yourdealership"
                  className="input-field w-full"
                  disabled={loading}
                />
                <p className="text-xs text-gray-500 mt-1">
                  Enter the full URL of your Facebook page (e.g., https://www.facebook.com/yourdealership)
                </p>
              </div>

              {/* Instagram URL */}
              <div>
                <label htmlFor="instagram_url" className="block font-medium mb-1">
                  Instagram Profile URL
                </label>
                <input
                  id="instagram_url"
                  type="url"
                  value={instagramUrl}
                  onChange={(e) => setInstagramUrl(e.target.value)}
                  placeholder="https://www.instagram.com/yourdealership"
                  className="input-field w-full"
                  disabled={loading}
                />
                <p className="text-xs text-gray-500 mt-1">
                  Enter the full URL of your Instagram profile (e.g., https://www.instagram.com/yourdealership)
                </p>
              </div>
            </div>

            {/* Promotional Panels Section */}
            <div className="border-t pt-6">
              <h2 className="text-xl font-semibold mb-4">Homepage Promotional Panels</h2>
              <p className="text-sm text-gray-600 mb-4">
                Configure promotional panels for Finance and Warranty sections that appear on the homepage below Customer Reviews.
              </p>

              {/* Finance Promo Panel */}
              <div className="mb-6 p-4 border rounded-lg bg-gray-50">
                <h3 className="font-semibold mb-3">Finance Promotional Panel</h3>
                
                {/* Finance Promo Image */}
                <div className="mb-4">
                  <label className="block font-medium mb-1">
                    Background Image
                  </label>
                  {financePromoImage ? (
                    <div className="mb-2">
                      <img 
                        src={financePromoImage} 
                        alt="Finance promo" 
                        className="w-full h-48 object-cover rounded-lg mb-2"
                      />
                      <button
                        type="button"
                        onClick={() => setFinancePromoImage('')}
                        className="text-red-600 hover:text-red-800 text-sm"
                        disabled={loading}
                      >
                        Remove Image
                      </button>
                    </div>
                  ) : (
                    <div>
                      <input
                        type="file"
                        accept="image/jpeg,image/png,image/webp"
                        onChange={handleFinancePromoImageUpload}
                        className="input-field w-full"
                        disabled={loading}
                      />
                      <p className="text-xs text-gray-500 mt-1">
                        Upload JPG, PNG, or WebP (max 5MB). Recommended size: 800x600px
                      </p>
                    </div>
                  )}
                </div>

                {/* Finance Promo Text */}
                <div>
                  <label htmlFor="finance_promo_text" className="block font-medium mb-1">
                    Promotional Text
                  </label>
                  <input
                    id="finance_promo_text"
                    type="text"
                    value={financePromoText}
                    onChange={(e) => setFinancePromoText(e.target.value)}
                    placeholder="e.g., Flexible Financing Options Available"
                    maxLength={500}
                    className="input-field w-full"
                    disabled={loading}
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    Text overlay displayed on the panel (max 500 characters)
                  </p>
                </div>
              </div>

              {/* Warranty Promo Panel */}
              <div className="mb-4 p-4 border rounded-lg bg-gray-50">
                <h3 className="font-semibold mb-3">Warranty Promotional Panel</h3>
                
                {/* Warranty Promo Image */}
                <div className="mb-4">
                  <label className="block font-medium mb-1">
                    Background Image
                  </label>
                  {warrantyPromoImage ? (
                    <div className="mb-2">
                      <img 
                        src={warrantyPromoImage} 
                        alt="Warranty promo" 
                        className="w-full h-48 object-cover rounded-lg mb-2"
                      />
                      <button
                        type="button"
                        onClick={() => setWarrantyPromoImage('')}
                        className="text-red-600 hover:text-red-800 text-sm"
                        disabled={loading}
                      >
                        Remove Image
                      </button>
                    </div>
                  ) : (
                    <div>
                      <input
                        type="file"
                        accept="image/jpeg,image/png,image/webp"
                        onChange={handleWarrantyPromoImageUpload}
                        className="input-field w-full"
                        disabled={loading}
                      />
                      <p className="text-xs text-gray-500 mt-1">
                        Upload JPG, PNG, or WebP (max 5MB). Recommended size: 800x600px
                      </p>
                    </div>
                  )}
                </div>

                {/* Warranty Promo Text */}
                <div>
                  <label htmlFor="warranty_promo_text" className="block font-medium mb-1">
                    Promotional Text
                  </label>
                  <input
                    id="warranty_promo_text"
                    type="text"
                    value={warrantyPromoText}
                    onChange={(e) => setWarrantyPromoText(e.target.value)}
                    placeholder="e.g., Comprehensive Warranty Coverage"
                    maxLength={500}
                    className="input-field w-full"
                    disabled={loading}
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    Text overlay displayed on the panel (max 500 characters)
                  </p>
                </div>
              </div>
            </div>

            {/* Finance Policy */}
            <div>
              <label htmlFor="finance_policy" className="block font-medium mb-1">
                Financing Options & Policy
              </label>
              <textarea
                id="finance_policy"
                rows="5"
                placeholder="Describe your financing options, terms, and application process..."
                className="input-field w-full resize-y"
                disabled={loading}
                {...register('finance_policy', {
                  maxLength: {
                    value: 2000,
                    message: 'Finance policy must be 2000 characters or less'
                  },
                  onChange: (e) => setFinancePolicyCount(e.target.value.length)
                })}
              />
              <p className="text-sm text-gray-600 mt-1">
                {financePolicyCount} / 2000 characters
              </p>
              {errors.finance_policy && (
                <p className="text-red-500 text-sm mt-1">{errors.finance_policy.message}</p>
              )}
            </div>

            {/* Warranty Policy */}
            <div>
              <label htmlFor="warranty_policy" className="block font-medium mb-1">
                Warranty Information & Coverage
              </label>
              <textarea
                id="warranty_policy"
                rows="5"
                placeholder="Describe your warranty coverage, terms, and conditions..."
                className="input-field w-full resize-y"
                disabled={loading}
                {...register('warranty_policy', {
                  maxLength: {
                    value: 2000,
                    message: 'Warranty policy must be 2000 characters or less'
                  },
                  onChange: (e) => setWarrantyPolicyCount(e.target.value.length)
                })}
              />
              <p className="text-sm text-gray-600 mt-1">
                {warrantyPolicyCount} / 2000 characters
              </p>
              {errors.warranty_policy && (
                <p className="text-red-500 text-sm mt-1">{errors.warranty_policy.message}</p>
              )}
            </div>

            {/* About Us */}
            <div>
              <label htmlFor="about" className="block font-medium mb-1">
                About Us
              </label>
              <textarea
                id="about"
                rows="5"
                placeholder="Tell customers about your dealership..."
                className="input-field w-full"
                disabled={loading}
                {...register('about', {
                  maxLength: {
                    value: 2000,
                    message: 'About must be 2000 characters or less'
                  }
                })}
              />
              {errors.about && (
                <p className="text-red-500 text-sm mt-1">{errors.about.message}</p>
              )}
            </div>

            {/* Submit Button */}
            {canEditSettings && (
              <div className="pt-4">
                <button
                  type="submit"
                  disabled={loading}
                  className="btn-primary w-full sm:w-auto"
                >
                  {loading ? 'Saving...' : 'Save Settings'}
                </button>
              </div>
            )}
          </form>
        </div>
      </div>

        {/* Navigation Manager Section (Story 5.2) - Wider Container */}
        <div className="max-w-7xl mx-auto mt-6">
          <div className="card bg-white p-6 shadow-md rounded-lg">
            <h2 className="text-2xl font-bold mb-4">Navigation Manager</h2>
            <p className="text-gray-600 mb-6">
              Customize your website's navigation buttons with icons and text. Drag to reorder, toggle to enable/disable, or add new navigation items.
            </p>
            {selectedDealership && (
              <NavigationManager
                dealership={selectedDealership}
                onSave={refreshDealership}
              />
            )}
          </div>
        </div>

        {/* EasyCars Integration Settings Section (Story 1.5) */}
        {canEditSettings && selectedDealership && (
          <div className="max-w-4xl mx-auto mt-6">
            <EasyCarsSettings dealershipId={selectedDealership.id} />
          </div>
        )}
      </div>
    </div>
  );
}

export default DealerSettings;

