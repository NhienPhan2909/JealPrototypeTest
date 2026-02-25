# Tech Stack

### Technology Stack for EasyCars Integration

| Category | Technology | Version | Purpose | Rationale |
|----------|-----------|---------|---------|-----------|
| Backend Language | C# | 12.0 (.NET 8) | EasyCars integration backend services | Existing system standard, mature ecosystem for enterprise integrations |
| Backend Framework | ASP.NET Core | 8.0 | REST API endpoints for credentials and sync | High performance, cross-platform, established in codebase |
| ORM | Entity Framework Core | 8.0 | Database access and migrations | Existing data access layer, strong migration support |
| Database | PostgreSQL | 15+ | Persistent storage for credentials and sync data | Existing database, ACID compliance, JSON support for API payloads |
| HTTP Client | HttpClient | .NET 8 | EasyCars API communication | Built-in, supports IHttpClientFactory for resilience |
| Background Jobs | Hangfire | 1.8+ | Scheduled and background sync operations | Persistent job storage, built-in retry logic, admin dashboard |
| Encryption | AES-256-GCM | .NET Cryptography | Credential encryption at rest | Industry standard, authenticated encryption prevents tampering |
| Authentication | JWT Bearer | .NET 8 Identity | Admin API authentication | Existing auth system, stateless, secure |
| Testing Framework | xUnit | 2.6+ | Unit and integration tests | Existing test framework in backend-dotnet |
| Mocking Library | Moq | 4.20+ | Mock dependencies in unit tests | Existing mocking framework, fluent API |
| Frontend Framework | React | 18+ | Admin UI for credentials and sync | Existing frontend framework |
| Frontend Language | TypeScript | 5+ | Type-safe frontend development | Existing frontend standard, catches errors at compile time |
| State Management | React Context/Hooks | React 18 | Component state and API calls | Built-in solution, sufficient for admin panel scope |
| HTTP Client (Frontend) | Axios | 1.6+ | API calls from React to backend | Existing HTTP library, interceptor support for auth |
| Logging | Serilog | 3.1+ | Structured logging for sync operations | Rich structured logging, multiple sinks support |
| Configuration | .NET Configuration | 8.0 | Environment-based settings | Built-in, supports appsettings.json and env vars |
| Code Analysis | SonarQube/Analyzer | Latest | Code quality and security scanning | Detect vulnerabilities, enforce code standards |

---
