# High Level Architecture

### Technical Summary

The EasyCars API Integration extends the existing monolithic .NET 8 application with a dedicated integration layer that manages bi-directional data synchronization between dealerships and the EasyCars platform. The architecture implements a background service pattern using Hangfire for scheduled synchronization jobs, HttpClient-based services for API communication with automatic JWT token management, and encrypted credential storage using AES-256-GCM encryption. Data flows through a mapping layer that transforms between EasyCars API models and domain entities, with comprehensive audit logging and idempotent sync operations to handle failures gracefully. The frontend React admin panel provides dealership administrators with secure credential management, manual sync triggers, and real-time synchronization status monitoring. This architecture maintains backward compatibility with existing vehicle and lead management features while enabling seamless integration with EasyCars Stock API and Lead API endpoints.

### Architecture Diagram

```mermaid
graph TB
    subgraph "Frontend - React Admin Panel"
        A[Admin UI<br/>EasyCars Settings]
        B[Sync Dashboard<br/>Status & Triggers]
        C[Vehicle/Lead Lists<br/>Enhanced Views]
    end
    
    subgraph "API Layer - ASP.NET Core"
        D[EasyCars Credentials<br/>Controller]
        E[EasyCars Sync<br/>Controller]
        F[Authentication<br/>Middleware]
    end
    
    subgraph "Application Layer - Use Cases"
        G[Manage Credentials<br/>Use Cases]
        H[Sync Stock<br/>Use Case]
        I[Sync Leads<br/>Use Case]
        J[Test Connection<br/>Use Case]
    end
    
    subgraph "Domain Layer - Entities"
        K[EasyCarsCredential<br/>Entity]
        L[EasyCarsSyncLog<br/>Entity]
        M[Vehicle Entity<br/>Extended]
        N[Lead Entity<br/>Extended]
    end
    
    subgraph "Infrastructure Layer - Services"
        O[EasyCarsApiClient<br/>HTTP Service]
        P[Credential Encryption<br/>Service]
        Q[Stock Mapper<br/>Service]
        R[Lead Mapper<br/>Service]
        S[Image Download<br/>Service]
    end
    
    subgraph "Background Jobs - Hangfire"
        T[Periodic Stock<br/>Sync Job]
        U[Periodic Lead<br/>Sync Job]
    end
    
    subgraph "Persistence Layer"
        V[(PostgreSQL<br/>EF Core)]
    end
    
    subgraph "External Services"
        W[EasyCars Stock API<br/>my.easycars.net.au]
        X[EasyCars Lead API<br/>my.easycars.net.au]
        Y[Image CDN<br/>EasyCars Images]
    end
    
    A --> D
    B --> E
    C --> E
    D --> F
    E --> F
    F --> G
    F --> H
    F --> I
    F --> J
    
    G --> K
    G --> P
    J --> O
    H --> O
    H --> Q
    I --> O
    I --> R
    
    O --> P
    O --> W
    O --> X
    H --> S
    S --> Y
    
    Q --> M
    R --> N
    
    K --> V
    L --> V
    M --> V
    N --> V
    
    T --> H
    U --> I
    
    style O fill:#e1f5ff
    style P fill:#fff4e1
    style T fill:#e8f5e9
    style U fill:#e8f5e9
    style W fill:#ffebee
    style X fill:#ffebee
```

### Architectural Patterns

The EasyCars integration follows established architectural patterns to ensure maintainability, testability, and scalability:

- **Clean Architecture (Domain-Centric):** Maintains strict dependency rules with Domain at the center, Application layer orchestrating use cases, Infrastructure implementing external concerns, and API exposing HTTP endpoints - _Rationale:_ Ensures business logic independence from infrastructure concerns and enables comprehensive testing

- **Repository Pattern:** Abstracts data access through repository interfaces defined in Application layer and implemented in Infrastructure - _Rationale:_ Provides flexibility for data access strategy changes and enables unit testing with mocked repositories

- **Background Job Pattern (Hangfire):** Executes long-running synchronization operations outside the HTTP request pipeline with persistence, retry logic, and monitoring - _Rationale:_ Prevents API timeouts, enables scheduled operations, and provides fault tolerance for sync jobs

- **API Client Service Pattern:** Encapsulates all EasyCars API communication in dedicated service classes with token management, error handling, and retry logic - _Rationale:_ Centralizes external API concerns, simplifies testing with mocked HTTP clients, and ensures consistent error handling

- **Data Mapper Pattern:** Transforms between external API models and domain entities through dedicated mapper services - _Rationale:_ Decouples domain model from external API structure, enabling independent evolution and comprehensive data validation

- **Encryption-at-Rest Pattern:** Encrypts sensitive credentials using AES-256-GCM before database storage with secure key management - _Rationale:_ Protects sensitive dealer credentials from database breaches and meets security compliance requirements

- **Audit Log Pattern:** Records all synchronization operations with timestamps, results, and error details for traceability - _Rationale:_ Enables troubleshooting, compliance auditing, and operational monitoring of integration health

- **Idempotent Operations Pattern:** Ensures sync operations can safely execute multiple times with same result - _Rationale:_ Handles retry scenarios, prevents duplicate data creation, and supports eventual consistency

---
