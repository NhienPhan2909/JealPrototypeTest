# Technical Assumptions

### Repository Structure

Monorepo - The EasyCars integration will be implemented within the existing monorepo structure with backend integration code in the backend/backend-dotnet directories and frontend admin interface enhancements in the frontend directory.

### Service Architecture

The integration will follow the existing Monolith architecture pattern with:
- RESTful API endpoints in the backend for credential management and sync operations
- Background job scheduler (using existing job infrastructure or implementing one) for periodic synchronization
- Service layer pattern isolating EasyCars API client logic from business logic
- Database models for storing EasyCars credentials and sync audit logs

### Testing Requirements

- Unit tests for EasyCars API client classes, credential encryption/decryption, and data mapping logic
- Integration tests using mock EasyCars API responses to test sync workflows end-to-end
- Manual testing convenience method to trigger sync operations with provided test credentials (PublicID: AA20EE61-5CFA-458D-9AFB-C4E929EA18E6, SecretKey: 7326AF23-714A-41A5-A74F-EC77B4E4F2F2)
- E2E tests covering admin interface credential management and manual sync trigger workflows

### Additional Technical Assumptions and Requests

- **API Client Library:** Implement a dedicated EasyCars API client class/module handling authentication token management, request/response serialization, and error handling
- **Environment Variable Support:** EasyCars API base URLs should be configurable via environment variables to support test/production environments and potential future staging environment
- **Database Schema:** New tables required: `dealership_easycar_credentials` (encrypted credentials), `easycar_stock_data` (complete Stock API response storage), `easycar_lead_data` (complete Lead API response storage), `easycar_sync_log` (audit trail of sync operations)
- **Background Job Framework:** Use existing job scheduler if available (e.g., Hangfire for .NET, node-cron for Node.js), or implement a simple scheduler for periodic sync execution
- **Idempotency:** Sync operations must be idempotent to handle duplicate execution scenarios safely
- **Data Retention:** Consider policy for EasyCars audit log retention (e.g., keep last 90 days of sync logs)
- **Observability:** Integrate with existing logging framework and consider adding metrics/monitoring for sync health dashboard
- **Credential Validation:** Implement "Test Connection" functionality allowing administrators to verify credentials before saving
- **Migration Path:** Provide migration scripts for existing dealerships to onboard to EasyCars integration without disrupting current operations
