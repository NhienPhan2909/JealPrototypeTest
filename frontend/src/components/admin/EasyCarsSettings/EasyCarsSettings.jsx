/**
 * EasyCarsSettings - Admin component for managing EasyCars integration credentials.
 * 
 * @component
 * @description Allows dealership admins to configure, test, update, and delete
 * EasyCars API credentials for automatic inventory synchronization.
 * 
 * Features:
 * - CRUD operations for credentials
 * - Test connection before saving
 * - Real-time validation
 * - Loading states and error handling
 * - Responsive design
 * - Credential masking for security
 * 
 * @example
 * <EasyCarsSettings dealershipId="123" />
 */

import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import CredentialStatusBadge from './CredentialStatusBadge';
import CredentialForm from './CredentialForm';
import DeleteConfirmDialog from './DeleteConfirmDialog';
import apiRequest from '../../../utils/api';

const EasyCarsSettings = ({ dealershipId }) => {
  // State management
  const [credentials, setCredentials] = useState(null);
  const [isConfigured, setIsConfigured] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isTesting, setIsTesting] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [testResult, setTestResult] = useState(null);
  const [error, setError] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  // Form management
  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isValid }
  } = useForm({
    mode: 'onBlur',
    defaultValues: {
      accountNumber: '',
      accountSecret: '',
      environment: 'Test',
      yardCode: ''
    }
  });

  const watchedFields = watch();

  /**
   * Fetch existing credentials on mount
   */
  useEffect(() => {
    fetchCredentials();
  }, [dealershipId]);

  /**
   * Auto-dismiss success and test result messages after 5 seconds
   */
  useEffect(() => {
    if (successMessage || testResult) {
      const timer = setTimeout(() => {
        setSuccessMessage('');
        setTestResult(null);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [successMessage, testResult]);

  /**
   * Fetch credentials from backend
   */
  const fetchCredentials = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await apiRequest('/api/admin/easycars/credentials');
      if (response.ok) {
        const data = await response.json();
        if (data) {
          setCredentials(data);
          setIsConfigured(true);
          reset({
            accountNumber: data.accountNumber || '',
            accountSecret: '', // Never pre-fill secret
            environment: data.environment || 'Test',
            yardCode: data.yardCode || ''
          });
        } else {
          setIsConfigured(false);
        }
      } else if (response.status === 404) {
        setIsConfigured(false);
      } else {
        throw new Error('Failed to fetch credentials');
      }
    } catch (err) {
      console.error('Error fetching credentials:', err);
      setError('Failed to load credentials. Please refresh the page.');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Test connection with provided credentials
   */
  const handleTestConnection = async () => {
    setIsTesting(true);
    setTestResult(null);
    setError(null);

    try {
      const response = await apiRequest('/api/admin/easycars/test-connection', {
        method: 'POST',
        body: JSON.stringify({
          accountNumber: watchedFields.accountNumber,
          accountSecret: watchedFields.accountSecret,
          environment: watchedFields.environment
        })
      });

      const result = await response.json();

      if (response.ok) {
        setTestResult(result);
      } else {
        setTestResult({
          success: false,
          message: result.message || 'Test connection failed',
          errorCode: result.errorCode || 'UNKNOWN'
        });
      }
    } catch (err) {
      console.error('Test connection error:', err);
      setTestResult({
        success: false,
        message: 'Network error. Please check your connection and try again.',
        errorCode: 'NETWORK_ERROR'
      });
    } finally {
      setIsTesting(false);
    }
  };

  /**
   * Save credentials (create or update)
   */
  const handleSave = async (formData) => {
    setIsSaving(true);
    setError(null);
    setSuccessMessage('');

    try {
      const method = credentials ? 'PUT' : 'POST';
      const url = credentials
        ? `/api/admin/easycars/credentials/${credentials.id}`
        : '/api/admin/easycars/credentials';

      const response = await apiRequest(url, {
        method,
        body: JSON.stringify({
          accountNumber: formData.accountNumber,
          accountSecret: formData.accountSecret,
          environment: formData.environment,
          yardCode: formData.yardCode || null,
          isActive: true
        })
      });

      if (response.ok) {
        setSuccessMessage(
          credentials ? 'Credentials updated successfully!' : 'Credentials saved successfully!'
        );
        setIsEditing(false);
        setTestResult(null);
        await fetchCredentials();

        // Clear the secret field for security
        reset({
          ...formData,
          accountSecret: ''
        });
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to save credentials');
      }
    } catch (err) {
      console.error('Save error:', err);
      setError(err.message || 'Failed to save credentials. Please try again.');
    } finally {
      setIsSaving(false);
    }
  };

  /**
   * Delete credentials
   */
  const handleDelete = async () => {
    setIsDeleting(true);
    setError(null);

    try {
      const response = await apiRequest(
        `/api/admin/easycars/credentials/${credentials.id}`,
        { method: 'DELETE' }
      );

      if (response.ok) {
        setSuccessMessage('Credentials deleted successfully.');
        setCredentials(null);
        setIsConfigured(false);
        setShowDeleteDialog(false);
        reset({
          accountNumber: '',
          accountSecret: '',
          environment: 'Test',
          yardCode: ''
        });
      } else {
        throw new Error('Failed to delete credentials');
      }
    } catch (err) {
      console.error('Delete error:', err);
      setError('Failed to delete credentials. Please try again.');
    } finally {
      setIsDeleting(false);
    }
  };

  /**
   * Handle update button click
   */
  const handleUpdateClick = () => {
    setIsEditing(true);
    setTestResult(null);
    setError(null);
  };

  /**
   * Handle cancel update
   */
  const handleCancelUpdate = () => {
    setIsEditing(false);
    setTestResult(null);
    setError(null);
    // Reset form to original values
    if (credentials) {
      reset({
        accountNumber: credentials.accountNumber || '',
        accountSecret: '',
        environment: credentials.environment || 'Test',
        yardCode: credentials.yardCode || ''
      });
    }
  };

  // Loading skeleton
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <div className="animate-pulse">
          <div className="h-6 bg-gray-200 rounded w-1/3 mb-4"></div>
          <div className="h-4 bg-gray-200 rounded w-2/3 mb-6"></div>
          <div className="space-y-4">
            <div className="h-10 bg-gray-200 rounded"></div>
            <div className="h-10 bg-gray-200 rounded"></div>
            <div className="h-10 bg-gray-200 rounded"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow">
      {/* Header */}
      <div className="border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              EasyCars Integration Settings
            </h2>
            <p className="mt-1 text-sm text-gray-600">
              Configure your dealership's connection to EasyCars for automatic inventory sync
            </p>
          </div>
          <CredentialStatusBadge isConfigured={isConfigured} />
        </div>
      </div>

      {/* Content */}
      <div className="px-6 py-6">
        {/* Success Message */}
        {successMessage && (
          <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-md flex items-start">
            <svg className="w-5 h-5 text-green-600 mr-3 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
            </svg>
            <p className="text-sm text-green-800">{successMessage}</p>
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-md flex items-start">
            <svg className="w-5 h-5 text-red-600 mr-3 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
            <p className="text-sm text-red-800">{error}</p>
          </div>
        )}

        {/* Configured View */}
        {isConfigured && !isEditing ? (
          <div>
            <div className="space-y-3 mb-6">
              <div>
                <span className="text-sm font-medium text-gray-700">Account Number:</span>
                <span className="ml-2 text-sm text-gray-900">{credentials?.accountNumber}</span>
              </div>
              <div>
                <span className="text-sm font-medium text-gray-700">Account Secret:</span>
                <span className="ml-2 text-sm text-gray-900">••••••••••••••••••••</span>
              </div>
              <div>
                <span className="text-sm font-medium text-gray-700">Environment:</span>
                <span className="ml-2 text-sm text-gray-900">{credentials?.environment}</span>
              </div>
              {credentials?.yardCode && (
                <div>
                  <span className="text-sm font-medium text-gray-700">Yard Code:</span>
                  <span className="ml-2 text-sm text-gray-900">{credentials.yardCode}</span>
                </div>
              )}
              {credentials?.updatedAt && (
                <div>
                  <span className="text-sm font-medium text-gray-700">Last Updated:</span>
                  <span className="ml-2 text-sm text-gray-900">
                    {new Date(credentials.updatedAt).toLocaleString()}
                  </span>
                </div>
              )}
            </div>

            <div className="flex space-x-3">
              <button
                type="button"
                onClick={handleUpdateClick}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                Update Credentials
              </button>
              <button
                type="button"
                onClick={() => setShowDeleteDialog(true)}
                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
              >
                Delete Credentials
              </button>
            </div>
          </div>
        ) : (
          /* Form View */
          <CredentialForm
            register={register}
            errors={errors}
            isValid={isValid}
            testResult={testResult}
            isTesting={isTesting}
            isSaving={isSaving}
            isEditing={isEditing}
            onTestConnection={handleTestConnection}
            onSubmit={handleSubmit(handleSave)}
            onCancel={isEditing ? handleCancelUpdate : null}
          />
        )}
      </div>

      {/* Delete Confirmation Dialog */}
      {showDeleteDialog && (
        <DeleteConfirmDialog
          isDeleting={isDeleting}
          onConfirm={handleDelete}
          onCancel={() => setShowDeleteDialog(false)}
        />
      )}
    </div>
  );
};

export default EasyCarsSettings;
