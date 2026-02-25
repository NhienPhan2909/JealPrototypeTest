/**
 * CredentialForm - Form component for entering and validating EasyCars credentials
 */

import { useState } from 'react';

const CredentialForm = ({
  register,
  errors,
  isValid,
  testResult,
  isTesting,
  isSaving,
  isEditing,
  onTestConnection,
  onSubmit,
  onCancel
}) => {
  const [showPassword, setShowPassword] = useState(false);
  const [showClientSecret, setShowClientSecret] = useState(false);
  const [allowSaveWithoutTest, setAllowSaveWithoutTest] = useState(false);

  const canSave = (testResult?.success || allowSaveWithoutTest) && isValid;



  return (
    <form onSubmit={onSubmit} className="space-y-6">
      {/* Client ID Field */}
      <div>
        <label htmlFor="clientId" className="block text-sm font-medium text-gray-700 mb-1">
          Client ID <span className="text-red-500">*</span>
        </label>
        <input
          id="clientId"
          type="text"
          placeholder="e.g. 45FEBB0B-C09A-4166-9C99-8A26F3E7316E"
          {...register('clientId', {
            required: 'Client ID is required'
          })}
          className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 ${
            errors.clientId
              ? 'border-red-300 focus:ring-red-500'
              : 'border-gray-300 focus:ring-blue-500'
          }`}
          disabled={isTesting || isSaving}
        />
        {errors.clientId && (
          <p className="mt-1 text-sm text-red-600">{errors.clientId.message}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">API authentication identifier provided by EasyCars</p>
      </div>

      {/* Client Secret Field */}
      <div>
        <label htmlFor="clientSecret" className="block text-sm font-medium text-gray-700 mb-1">
          Client Secret <span className="text-red-500">*</span>
        </label>
        <div className="relative">
          <input
            id="clientSecret"
            type={showClientSecret ? 'text' : 'password'}
            placeholder="••••••••••••••••"
            {...register('clientSecret', {
              required: 'Client Secret is required'
            })}
            className={`w-full px-3 py-2 pr-10 border rounded-md focus:outline-none focus:ring-2 ${
              errors.clientSecret
                ? 'border-red-300 focus:ring-red-500'
                : 'border-gray-300 focus:ring-blue-500'
            }`}
            disabled={isTesting || isSaving}
          />
          <button
            type="button"
            onClick={() => setShowClientSecret(!showClientSecret)}
            className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 focus:outline-none"
            disabled={isTesting || isSaving}
            aria-label={showClientSecret ? 'Hide client secret' : 'Show client secret'}
          >
            {showClientSecret ? (
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
              </svg>
            ) : (
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
            )}
          </button>
        </div>
        {errors.clientSecret && (
          <p className="mt-1 text-sm text-red-600">{errors.clientSecret.message}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">API authentication secret provided by EasyCars (never shown after saving)</p>
      </div>

      {/* Account Number Field */}
      <div>
        <label htmlFor="accountNumber" className="block text-sm font-medium text-gray-700 mb-1">
          Account Number (PublicID) <span className="text-red-500">*</span>
        </label>
        <input
          id="accountNumber"
          type="text"
          placeholder="e.g. EC114575"
          {...register('accountNumber', {
            required: 'Account Number is required'
          })}
          className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 ${
            errors.accountNumber
              ? 'border-red-300 focus:ring-red-500'
              : 'border-gray-300 focus:ring-blue-500'
          }`}
          disabled={isTesting || isSaving}
        />
        {errors.accountNumber && (
          <p className="mt-1 text-sm text-red-600">{errors.accountNumber.message}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">Your EasyCars account identifier</p>
      </div>

      {/* Account Secret Field */}
      <div>
        <label htmlFor="accountSecret" className="block text-sm font-medium text-gray-700 mb-1">
          Account Secret (SecretKey) <span className="text-red-500">*</span>
        </label>
        <div className="relative">
          <input
            id="accountSecret"
            type={showPassword ? 'text' : 'password'}
            placeholder="••••••••••••••••••••••••"
            {...register('accountSecret', {
              required: 'Account Secret is required'
            })}
            className={`w-full px-3 py-2 pr-10 border rounded-md focus:outline-none focus:ring-2 ${
              errors.accountSecret
                ? 'border-red-300 focus:ring-red-500'
                : 'border-gray-300 focus:ring-blue-500'
            }`}
            disabled={isTesting || isSaving}
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 focus:outline-none"
            disabled={isTesting || isSaving}
            aria-label={showPassword ? 'Hide password' : 'Show password'}
          >
            {showPassword ? (
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
              </svg>
            ) : (
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
            )}
          </button>
        </div>
        {errors.accountSecret && (
          <p className="mt-1 text-sm text-red-600">{errors.accountSecret.message}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">
          Your EasyCars secret key (never shown after saving)
        </p>
      </div>

      {/* Environment and Test Connection Row */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Environment Field */}
        <div>
          <label htmlFor="environment" className="block text-sm font-medium text-gray-700 mb-1">
            Environment <span className="text-red-500">*</span>
          </label>
          <select
            id="environment"
            {...register('environment', { required: 'Environment is required' })}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={isTesting || isSaving}
          >
            <option value="Test">Test</option>
            <option value="Production">Production</option>
          </select>
          {errors.environment && (
            <p className="mt-1 text-sm text-red-600">{errors.environment.message}</p>
          )}
          <p className="mt-1 text-sm text-gray-500">
            Select 'Test' for testing, 'Production' for live sync
          </p>
        </div>

        {/* Test Connection Button */}
        <div className="flex items-end">
          <button
            type="button"
            onClick={onTestConnection}
            disabled={!isValid || isTesting || isSaving}
            className="w-full px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-500 disabled:bg-gray-300 disabled:cursor-not-allowed flex items-center justify-center"
          >
            {isTesting ? (
              <>
                <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Testing...
              </>
            ) : (
              <>
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
                Test Connection
              </>
            )}
          </button>
        </div>
      </div>

      {/* Test Result Message */}
      {testResult && (
        <div
          className={`p-4 rounded-md flex items-start ${
            testResult.success
              ? 'bg-green-50 border border-green-200'
              : 'bg-red-50 border border-red-200'
          }`}
        >
          <svg
            className={`w-5 h-5 mr-3 mt-0.5 ${
              testResult.success ? 'text-green-600' : 'text-red-600'
            }`}
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            {testResult.success ? (
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
            ) : (
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            )}
          </svg>
          <div className="flex-1">
            <p className={`text-sm font-medium ${testResult.success ? 'text-green-800' : 'text-red-800'}`}>
              {testResult.message}
            </p>
            {testResult.details && (
              <p className={`mt-1 text-sm ${testResult.success ? 'text-green-700' : 'text-red-700'}`}>
                {testResult.details}
              </p>
            )}
          </div>
        </div>
      )}

      {/* Yard Code Field (Optional) */}
      <div>
        <label htmlFor="yardCode" className="block text-sm font-medium text-gray-700 mb-1">
          Yard Code (Optional)
        </label>
        <input
          id="yardCode"
          type="text"
          placeholder="MAIN"
          {...register('yardCode', {
            maxLength: {
              value: 50,
              message: 'Yard Code must be 50 characters or less'
            }
          })}
          className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 ${
            errors.yardCode
              ? 'border-red-300 focus:ring-red-500'
              : 'border-gray-300 focus:ring-blue-500'
          }`}
          disabled={isTesting || isSaving}
        />
        {errors.yardCode && (
          <p className="mt-1 text-sm text-red-600">{errors.yardCode.message}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">
          Optional yard identifier for multi-location dealerships
        </p>
      </div>

      {/* Save Without Test Checkbox */}
      {!testResult?.success && (
        <div className="flex items-start">
          <input
            id="allowSaveWithoutTest"
            type="checkbox"
            checked={allowSaveWithoutTest}
            onChange={(e) => setAllowSaveWithoutTest(e.target.checked)}
            className="mt-1 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            disabled={isTesting || isSaving}
          />
          <label htmlFor="allowSaveWithoutTest" className="ml-2 block text-sm text-gray-700">
            Save without testing (not recommended)
            <span className="block text-xs text-yellow-600 mt-1">
              ⚠️ Saving without testing may result in sync failures
            </span>
          </label>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex items-center space-x-3 pt-4 border-t border-gray-200">
        <button
          type="submit"
          disabled={!canSave || isSaving}
          className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-300 disabled:cursor-not-allowed flex items-center"
        >
          {isSaving ? (
            <>
              <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Saving...
            </>
          ) : (
            'Save Credentials'
          )}
        </button>

        {isEditing && onCancel && (
          <button
            type="button"
            onClick={onCancel}
            disabled={isSaving}
            className="px-6 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500 disabled:opacity-50"
          >
            Cancel
          </button>
        )}

        {!testResult?.success && !allowSaveWithoutTest && (
          <p className="text-sm text-gray-500 italic">
            Test connection first to enable save
          </p>
        )}
      </div>
    </form>
  );
};

export default CredentialForm;
