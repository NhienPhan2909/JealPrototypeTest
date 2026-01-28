# .NET Migration Implementation Guide

**Created:** 2026-01-27  
**Purpose:** Step-by-step guide to implement .NET backend with Clean Architecture & DDD  
**Prerequisites:** .NET 8.0 SDK, Visual Studio 2022 or VS Code with C# extension

---

## Table of Contents

1. [Project Setup](#project-setup)
2. [Solution Structure](#solution-structure)
3. [Domain Layer Implementation](#domain-layer-implementation)
4. [Application Layer Implementation](#application-layer-implementation)
5. [Infrastructure Layer Implementation](#infrastructure-layer-implementation)
6. [API Layer Implementation](#api-layer-implementation)
7. [Configuration & Startup](#configuration--startup)
8. [Testing Strategy](#testing-strategy)

---

## Project Setup

### Step 1: Install .NET 8.0 SDK

Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

Verify installation:
```bash
dotnet --version
```

### Step 2: Create Solution and Projects

```bash
# Navigate to project root
cd D:\JealPrototypeTest\JealPrototypeTest

# Create backend-dotnet directory
mkdir backend-dotnet
cd backend-dotnet

# Create solution
dotnet new sln -n JealPrototype

# Create Domain layer (class library)
dotnet new classlib -n JealPrototype.Domain -f net8.0
dotnet sln add JealPrototype.Domain/JealPrototype.Domain.csproj

# Create Application layer (class library)
dotnet new classlib -n JealPrototype.Application -f net8.0
dotnet sln add JealPrototype.Application/JealPrototype.Application.csproj

# Create Infrastructure layer (class library)
dotnet new classlib -n JealPrototype.Infrastructure -f net8.0
dotnet sln add JealPrototype.Infrastructure/JealPrototype.Infrastructure.csproj

# Create API layer (web api)
dotnet new webapi -n JealPrototype.API -f net8.0
dotnet sln add JealPrototype.API/JealPrototype.API.csproj

# Create Tests project
dotnet new xunit -n JealPrototype.Tests -f net8.0
dotnet sln add JealPrototype.Tests/JealPrototype.Tests.csproj

# Add project references
cd JealPrototype.Application
dotnet add reference ../JealPrototype.Domain/JealPrototype.Domain.csproj

cd ../JealPrototype.Infrastructure
dotnet add reference ../JealPrototype.Domain/JealPrototype.Domain.csproj
dotnet add reference ../JealPrototype.Application/JealPrototype.Application.csproj

cd ../JealPrototype.API
dotnet add reference ../JealPrototype.Application/JealPrototype.Application.csproj
dotnet add reference ../JealPrototype.Infrastructure/JealPrototype.Infrastructure.csproj

cd ../JealPrototype.Tests
dotnet add reference ../JealPrototype.Domain/JealPrototype.Domain.csproj
dotnet add reference ../JealPrototype.Application/JealPrototype.Application.csproj
dotnet add reference ../JealPrototype.Infrastructure/JealPrototype.Infrastructure.csproj
dotnet add reference ../JealPrototype.API/JealPrototype.API.csproj

cd ..
```

### Step 3: Install NuGet Packages

```bash
# Domain layer (no dependencies - keep it pure)

# Application layer
cd JealPrototype.Application
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package AutoMapper
cd ..

# Infrastructure layer
cd JealPrototype.Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package BCrypt.Net-Next
dotnet add package CloudinaryDotNet
dotnet add package MailKit
dotnet add package MimeKit
cd ..

# API layer
cd JealPrototype.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Cors
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
cd ..

# Tests layer
cd JealPrototype.Tests
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
cd ..
```

---

## Solution Structure

```
backend-dotnet/
├── JealPrototype.sln
├── src/
│   ├── JealPrototype.Domain/
│   │   ├── Entities/
│   │   │   ├── Dealership.cs
│   │   │   ├── Vehicle.cs
│   │   │   ├── Lead.cs
│   │   │   ├── SalesRequest.cs
│   │   │   ├── User.cs
│   │   │   ├── BlogPost.cs
│   │   │   └── DesignTemplate.cs
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   ├── PhoneNumber.cs
│   │   │   ├── HexColor.cs
│   │   │   └── NavigationConfig.cs
│   │   ├── Enums/
│   │   │   ├── UserType.cs
│   │   │   ├── VehicleCondition.cs
│   │   │   ├── VehicleStatus.cs
│   │   │   ├── LeadStatus.cs
│   │   │   ├── SalesRequestStatus.cs
│   │   │   ├── BlogPostStatus.cs
│   │   │   ├── HeroType.cs
│   │   │   └── Permission.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   ├── EntityNotFoundException.cs
│   │   │   └── ValidationException.cs
│   │   └── Interfaces/
│   │       ├── IRepository.cs
│   │       ├── IDealershipRepository.cs
│   │       ├── IVehicleRepository.cs
│   │       ├── ILeadRepository.cs
│   │       ├── ISalesRequestRepository.cs
│   │       ├── IUserRepository.cs
│   │       ├── IBlogPostRepository.cs
│   │       └── IDesignTemplateRepository.cs
│   │
│   ├── JealPrototype.Application/
│   │   ├── DTOs/
│   │   │   ├── Dealership/
│   │   │   │   ├── DealershipDto.cs
│   │   │   │   ├── CreateDealershipDto.cs
│   │   │   │   └── UpdateDealershipDto.cs
│   │   │   ├── Vehicle/
│   │   │   │   ├── VehicleDto.cs
│   │   │   │   ├── CreateVehicleDto.cs
│   │   │   │   ├── UpdateVehicleDto.cs
│   │   │   │   └── VehicleFilterDto.cs
│   │   │   ├── Lead/
│   │   │   ├── SalesRequest/
│   │   │   ├── User/
│   │   │   ├── BlogPost/
│   │   │   ├── DesignTemplate/
│   │   │   └── Auth/
│   │   │       ├── LoginRequestDto.cs
│   │   │       └── LoginResponseDto.cs
│   │   ├── UseCases/
│   │   │   ├── Dealerships/
│   │   │   │   ├── GetAllDealershipsUseCase.cs
│   │   │   │   ├── GetDealershipByIdUseCase.cs
│   │   │   │   ├── CreateDealershipUseCase.cs
│   │   │   │   ├── UpdateDealershipUseCase.cs
│   │   │   │   └── DeleteDealershipUseCase.cs
│   │   │   ├── Vehicles/
│   │   │   │   ├── GetVehiclesUseCase.cs
│   │   │   │   ├── GetVehicleByIdUseCase.cs
│   │   │   │   ├── CreateVehicleUseCase.cs
│   │   │   │   ├── UpdateVehicleUseCase.cs
│   │   │   │   └── DeleteVehicleUseCase.cs
│   │   │   ├── Leads/
│   │   │   ├── SalesRequests/
│   │   │   ├── Users/
│   │   │   ├── BlogPosts/
│   │   │   └── Auth/
│   │   │       ├── LoginUseCase.cs
│   │   │       └── GetCurrentUserUseCase.cs
│   │   ├── Validators/
│   │   │   ├── CreateDealershipValidator.cs
│   │   │   ├── CreateVehicleValidator.cs
│   │   │   └── ... (validators for all DTOs)
│   │   ├── Mapping/
│   │   │   └── MappingProfile.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IEmailService.cs
│   │   │   ├── IImageUploadService.cs
│   │   │   └── IGoogleReviewsService.cs
│   │   └── Common/
│   │       ├── Result.cs
│   │       └── PagedResult.cs
│   │
│   ├── JealPrototype.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── DealershipConfiguration.cs
│   │   │   │   ├── VehicleConfiguration.cs
│   │   │   │   ├── LeadConfiguration.cs
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   └── ... (entity configurations)
│   │   │   └── Repositories/
│   │   │       ├── DealershipRepository.cs
│   │   │       ├── VehicleRepository.cs
│   │   │       ├── LeadRepository.cs
│   │   │       └── ... (repository implementations)
│   │   ├── Services/
│   │   │   ├── JwtAuthService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── CloudinaryImageUploadService.cs
│   │   │   └── GoogleReviewsService.cs
│   │   ├── Migrations/
│   │   │   └── (EF Core migrations - empty, using existing DB)
│   │   └── DependencyInjection.cs
│   │
│   └── JealPrototype.API/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── DealershipsController.cs
│       │   ├── VehiclesController.cs
│       │   ├── LeadsController.cs
│       │   ├── SalesRequestsController.cs
│       │   ├── UsersController.cs
│       │   ├── BlogsController.cs
│       │   ├── DesignTemplatesController.cs
│       │   ├── UploadController.cs
│       │   └── GoogleReviewsController.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── DealershipScopeMiddleware.cs
│       ├── Filters/
│       │   ├── CaptchaVerificationFilter.cs
│       │   └── DealershipScopeFilter.cs
│       ├── Authorization/
│       │   ├── PermissionRequirement.cs
│       │   ├── PermissionHandler.cs
│       │   └── DealershipScopeRequirement.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
└── tests/
    └── JealPrototype.Tests/
        ├── Unit/
        │   ├── Domain/
        │   ├── Application/
        │   └── Infrastructure/
        └── Integration/
            └── Controllers/
```

---

## Domain Layer Implementation

### 1. Create Base Entity

**File:** `JealPrototype.Domain/Entities/BaseEntity.cs`

```csharp
namespace JealPrototype.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
```

### 2. Create Enums

**File:** `JealPrototype.Domain/Enums/UserType.cs`

```csharp
namespace JealPrototype.Domain.Enums;

public enum UserType
{
    Admin,
    DealershipOwner,
    DealershipStaff
}
```

**File:** `JealPrototype.Domain/Enums/VehicleCondition.cs`

```csharp
namespace JealPrototype.Domain.Enums;

public enum VehicleCondition
{
    New,
    Used
}
```

**File:** `JealPrototype.Domain/Enums/VehicleStatus.cs`

```csharp
namespace JealPrototype.Domain.Enums;

public enum VehicleStatus
{
    Draft,
    Active,
    Pending,
    Sold
}
```

**File:** `JealPrototype.Domain/Enums/LeadStatus.cs`

```csharp
namespace JealPrototype.Domain.Enums;

public enum LeadStatus
{
    Received,
    InProgress,
    Done
}
```

**File:** `JealPrototype.Domain/Enums/Permission.cs`

```csharp
namespace JealPrototype.Domain.Enums;

public enum Permission
{
    Vehicles,
    Leads,
    SalesRequests,
    Blogs,
    Settings
}
```

**File:** `JealPrototype.Domain/Enums/HeroType.cs`

```csharp
namespace JealPrototype.Domain.Enums;

public enum HeroType
{
    Image,
    Video,
    Carousel
}
```

### 3. Create Value Objects

**File:** `JealPrototype.Domain/ValueObjects/Email.cs`

```csharp
using System.Text.RegularExpressions;

namespace JealPrototype.Domain.ValueObjects;

public class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (email.Length > 255)
            throw new ArgumentException("Email must be 255 characters or less", nameof(email));

        return new Email(email);
    }

    public bool Equals(Email? other) => other != null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
```

**File:** `JealPrototype.Domain/ValueObjects/HexColor.cs`

```csharp
using System.Text.RegularExpressions;

namespace JealPrototype.Domain.ValueObjects;

public class HexColor : IEquatable<HexColor>
{
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    public string Value { get; }

    private HexColor(string value)
    {
        Value = value;
    }

    public static HexColor Create(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("Color cannot be empty", nameof(color));

        if (!HexColorRegex.IsMatch(color))
            throw new ArgumentException("Invalid hex color format. Use #RRGGBB or #RGB", nameof(color));

        return new HexColor(color);
    }

    public bool Equals(HexColor? other) => other != null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is HexColor color && Equals(color);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;

    public static implicit operator string(HexColor color) => color.Value;
}
```

### 4. Create Domain Entities

**File:** `JealPrototype.Domain/Entities/Dealership.cs`

```csharp
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Domain.Entities;

public class Dealership : BaseEntity
{
    public string Name { get; private set; }
    public string? LogoUrl { get; private set; }
    public string Address { get; private set; }
    public string Phone { get; private set; }
    public Email Email { get; private set; }
    public string? Hours { get; private set; }
    public string? About { get; private set; }
    public string? WebsiteUrl { get; private set; }
    
    // Policies
    public string? FinancePolicy { get; private set; }
    public string? WarrantyPolicy { get; private set; }
    
    // Hero Section
    public string? HeroBackgroundImage { get; private set; }
    public HeroType HeroType { get; private set; } = HeroType.Image;
    public string? HeroVideoUrl { get; private set; }
    public List<string> HeroCarouselImages { get; private set; } = new();
    
    // Branding
    public HexColor ThemeColor { get; private set; }
    public HexColor SecondaryThemeColor { get; private set; }
    public HexColor BodyBackgroundColor { get; private set; }
    public string FontFamily { get; private set; } = "system";
    
    // Navigation
    public string? NavigationConfigJson { get; private set; }
    
    // Social Media
    public string? FacebookUrl { get; private set; }
    public string? InstagramUrl { get; private set; }
    
    // Promotional Panels
    public string? FinancePromoImage { get; private set; }
    public string? FinancePromoText { get; private set; }
    public string? WarrantyPromoImage { get; private set; }
    public string? WarrantyPromoText { get; private set; }

    // Navigation properties
    public ICollection<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
    public ICollection<Lead> Leads { get; private set; } = new List<Lead>();
    public ICollection<User> Users { get; private set; } = new List<User>();

    private Dealership() { } // EF Core constructor

    public static Dealership Create(
        string name,
        string address,
        string phone,
        string email,
        string? logoUrl = null,
        string? hours = null,
        string? about = null,
        string? websiteUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 255)
            throw new ArgumentException("Name is required and must be 255 characters or less");

        if (string.IsNullOrWhiteSpace(address) || address.Length > 255)
            throw new ArgumentException("Address is required and must be 255 characters or less");

        if (string.IsNullOrWhiteSpace(phone) || phone.Length > 20)
            throw new ArgumentException("Phone is required and must be 20 characters or less");

        return new Dealership
        {
            Name = name,
            Address = address,
            Phone = phone,
            Email = Email.Create(email),
            LogoUrl = logoUrl,
            Hours = hours,
            About = about,
            WebsiteUrl = websiteUrl,
            ThemeColor = HexColor.Create("#3B82F6"),
            SecondaryThemeColor = HexColor.Create("#FFFFFF"),
            BodyBackgroundColor = HexColor.Create("#FFFFFF")
        };
    }

    public void Update(
        string name,
        string address,
        string phone,
        string email,
        string? logoUrl = null,
        string? hours = null,
        string? about = null,
        string? websiteUrl = null)
    {
        Name = name;
        Address = address;
        Phone = phone;
        Email = Email.Create(email);
        LogoUrl = logoUrl;
        Hours = hours;
        About = about;
        WebsiteUrl = websiteUrl;
    }

    public void UpdateBranding(string themeColor, string secondaryThemeColor, string bodyBackgroundColor, string fontFamily)
    {
        ThemeColor = HexColor.Create(themeColor);
        SecondaryThemeColor = HexColor.Create(secondaryThemeColor);
        BodyBackgroundColor = HexColor.Create(bodyBackgroundColor);
        FontFamily = fontFamily;
    }
}
```

**File:** `JealPrototype.Domain/Entities/Vehicle.cs`

```csharp
using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Entities;

public class Vehicle : BaseEntity
{
    public int DealershipId { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public decimal Price { get; private set; }
    public int Mileage { get; private set; }
    public VehicleCondition Condition { get; private set; }
    public VehicleStatus Status { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public List<string> Images { get; private set; } = new();

    // Navigation property
    public Dealership Dealership { get; private set; } = null!;

    private Vehicle() { } // EF Core constructor

    public static Vehicle Create(
        int dealershipId,
        string make,
        string model,
        int year,
        decimal price,
        int mileage,
        VehicleCondition condition,
        VehicleStatus status,
        string title,
        string? description = null,
        List<string>? images = null)
    {
        if (dealershipId <= 0)
            throw new ArgumentException("Invalid dealership ID", nameof(dealershipId));

        if (string.IsNullOrWhiteSpace(make) || make.Length > 100)
            throw new ArgumentException("Make is required and must be 100 characters or less", nameof(make));

        if (string.IsNullOrWhiteSpace(model) || model.Length > 100)
            throw new ArgumentException("Model is required and must be 100 characters or less", nameof(model));

        if (year < 1900 || year > 2100)
            throw new ArgumentException("Year must be between 1900 and 2100", nameof(year));

        if (price < 0)
            throw new ArgumentException("Price must be non-negative", nameof(price));

        if (mileage < 0)
            throw new ArgumentException("Mileage must be non-negative", nameof(mileage));

        if (string.IsNullOrWhiteSpace(title) || title.Length > 255)
            throw new ArgumentException("Title is required and must be 255 characters or less", nameof(title));

        return new Vehicle
        {
            DealershipId = dealershipId,
            Make = make,
            Model = model,
            Year = year,
            Price = price,
            Mileage = mileage,
            Condition = condition,
            Status = status,
            Title = title,
            Description = description,
            Images = images ?? new List<string>()
        };
    }

    public void Update(
        string make,
        string model,
        int year,
        decimal price,
        int mileage,
        VehicleCondition condition,
        VehicleStatus status,
        string title,
        string? description = null,
        List<string>? images = null)
    {
        Make = make;
        Model = model;
        Year = year;
        Price = price;
        Mileage = mileage;
        Condition = condition;
        Status = status;
        Title = title;
        Description = description;
        if (images != null)
            Images = images;
    }

    public void UpdateStatus(VehicleStatus newStatus)
    {
        Status = newStatus;
    }
}
```

**File:** `JealPrototype.Domain/Entities/User.cs`

```csharp
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public Email Email { get; private set; }
    public string FullName { get; private set; }
    public UserType UserType { get; private set; }
    public int? DealershipId { get; private set; }
    public List<Permission> Permissions { get; private set; } = new();
    public int? CreatedBy { get; private set; }

    // Navigation property
    public Dealership? Dealership { get; private set; }

    private User() { } // EF Core constructor

    public static User Create(
        string username,
        string passwordHash,
        string email,
        string fullName,
        UserType userType,
        int? dealershipId = null,
        List<Permission>? permissions = null,
        int? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length > 100)
            throw new ArgumentException("Username is required and must be 100 characters or less", nameof(username));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 255)
            throw new ArgumentException("Full name is required and must be 255 characters or less", nameof(fullName));

        if (userType == UserType.Admin && dealershipId.HasValue)
            throw new ArgumentException("Admin users cannot have a dealership ID", nameof(dealershipId));

        if ((userType == UserType.DealershipOwner || userType == UserType.DealershipStaff) && !dealershipId.HasValue)
            throw new ArgumentException("Dealership users must have a dealership ID", nameof(dealershipId));

        return new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = Email.Create(email),
            FullName = fullName,
            UserType = userType,
            DealershipId = dealershipId,
            Permissions = permissions ?? new List<Permission>(),
            CreatedBy = createdBy
        };
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void UpdateProfile(string email, string fullName)
    {
        Email = Email.Create(email);
        FullName = fullName;
    }

    public void UpdatePermissions(List<Permission> permissions)
    {
        if (UserType != UserType.DealershipStaff)
            throw new InvalidOperationException("Only staff users have configurable permissions");

        Permissions = permissions;
    }

    public bool HasPermission(Permission permission)
    {
        if (UserType == UserType.Admin || UserType == UserType.DealershipOwner)
            return true; // Admins and owners have all permissions

        return Permissions.Contains(permission);
    }
}
```

### 5. Create Repository Interfaces

**File:** `JealPrototype.Domain/Interfaces/IRepository.cs`

```csharp
namespace JealPrototype.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

**File:** `JealPrototype.Domain/Interfaces/IDealershipRepository.cs`

```csharp
using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IDealershipRepository : IRepository<Dealership>
{
    Task<Dealership?> GetByWebsiteUrlAsync(string websiteUrl, CancellationToken cancellationToken = default);
}
```

**File:** `JealPrototype.Domain/Interfaces/IVehicleRepository.cs`

```csharp
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetByDealershipIdAsync(
        int dealershipId,
        VehicleStatus? status = null,
        string? brand = null,
        int? minYear = null,
        int? maxYear = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
}
```

**File:** `JealPrototype.Domain/Interfaces/IUserRepository.cs`

```csharp
using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
}
```

---

## Application Layer Implementation

### 1. Create DTOs

**File:** `JealPrototype.Application/DTOs/Dealership/CreateDealershipDto.cs`

```csharp
namespace JealPrototype.Application.DTOs.Dealership;

public record CreateDealershipDto(
    string Name,
    string Address,
    string Phone,
    string Email,
    string? LogoUrl,
    string? Hours,
    string? About,
    string? WebsiteUrl
);
```

**File:** `JealPrototype.Application/DTOs/Dealership/DealershipDto.cs`

```csharp
namespace JealPrototype.Application.DTOs.Dealership;

public record DealershipDto(
    int Id,
    string Name,
    string? LogoUrl,
    string Address,
    string Phone,
    string Email,
    string? Hours,
    string? About,
    string? WebsiteUrl,
    string ThemeColor,
    string SecondaryThemeColor,
    string BodyBackgroundColor,
    string FontFamily,
    DateTime CreatedAt
);
```

**File:** `JealPrototype.Application/DTOs/Vehicle/CreateVehicleDto.cs`

```csharp
using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.DTOs.Vehicle;

public record CreateVehicleDto(
    int DealershipId,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    VehicleCondition Condition,
    VehicleStatus Status,
    string Title,
    string? Description,
    List<string>? Images
);
```

**File:** `JealPrototype.Application/DTOs/Auth/LoginRequestDto.cs`

```csharp
namespace JealPrototype.Application.DTOs.Auth;

public record LoginRequestDto(
    string Username,
    string Password
);
```

**File:** `JealPrototype.Application/DTOs/Auth/LoginResponseDto.cs`

```csharp
using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.DTOs.Auth;

public record LoginResponseDto(
    string Token,
    UserDto User
);

public record UserDto(
    int Id,
    string Username,
    string Email,
    string FullName,
    UserType UserType,
    int? DealershipId,
    List<Permission> Permissions
);
```

### 2. Create Use Cases

**File:** `JealPrototype.Application/UseCases/Dealerships/GetAllDealershipsUseCase.cs`

```csharp
using AutoMapper;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Dealerships;

public class GetAllDealershipsUseCase
{
    private readonly IDealershipRepository _repository;
    private readonly IMapper _mapper;

    public GetAllDealershipsUseCase(IDealershipRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DealershipDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dealerships = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<DealershipDto>>(dealerships);
    }
}
```

**File:** `JealPrototype.Application/UseCases/Auth/LoginUseCase.cs`

```csharp
using JealPrototype.Application.DTOs.Auth;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public LoginUseCase(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<LoginResponseDto?> ExecuteAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null)
            return null;

        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            return null;

        var token = _authService.GenerateJwtToken(user);

        return new LoginResponseDto(
            token,
            new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                user.UserType,
                user.DealershipId,
                user.Permissions
            )
        );
    }
}
```

### 3. Create Validators

**File:** `JealPrototype.Application/Validators/CreateDealershipValidator.cs`

```csharp
using FluentValidation;
using JealPrototype.Application.DTOs.Dealership;

namespace JealPrototype.Application.Validators;

public class CreateDealershipValidator : AbstractValidator<CreateDealershipDto>
{
    public CreateDealershipValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must be 255 characters or less");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(255).WithMessage("Address must be 255 characters or less");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(20).WithMessage("Phone must be 20 characters or less");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must be 255 characters or less");
    }
}
```

### 4. Create AutoMapper Profile

**File:** `JealPrototype.Application/Mapping/MappingProfile.cs`

```csharp
using AutoMapper;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Dealership mappings
        CreateMap<Dealership, DealershipDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.ThemeColor, opt => opt.MapFrom(src => src.ThemeColor.Value))
            .ForMember(dest => dest.SecondaryThemeColor, opt => opt.MapFrom(src => src.SecondaryThemeColor.Value))
            .ForMember(dest => dest.BodyBackgroundColor, opt => opt.MapFrom(src => src.BodyBackgroundColor.Value));

        // Vehicle mappings
        CreateMap<Vehicle, VehicleDto>();
        
        // Add more mappings...
    }
}
```

---

## Infrastructure Layer Implementation

### 1. Create DbContext

**File:** `JealPrototype.Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using System.Reflection;

namespace JealPrototype.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Dealership> Dealerships => Set<Dealership>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<SalesRequest> SalesRequests => Set<SalesRequest>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<DesignTemplate> DesignTemplates => Set<DesignTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### 2. Create Entity Configurations

**File:** `JealPrototype.Infrastructure/Persistence/Configurations/DealershipConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class DealershipConfiguration : IEntityTypeConfiguration<Dealership>
{
    public void Configure(EntityTypeBuilder<Dealership> builder)
    {
        builder.ToTable("dealership");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(d => d.LogoUrl)
            .HasColumnName("logo_url");

        builder.Property(d => d.Address)
            .IsRequired()
            .HasColumnName("address");

        builder.Property(d => d.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("phone");

        // Value object mapping for Email
        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email")
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));

        builder.Property(d => d.Hours)
            .HasColumnName("hours");

        builder.Property(d => d.About)
            .HasColumnName("about");

        builder.Property(d => d.WebsiteUrl)
            .HasMaxLength(255)
            .HasColumnName("website_url");

        builder.HasIndex(d => d.WebsiteUrl)
            .IsUnique();

        // Value object mapping for HexColor
        builder.Property(d => d.ThemeColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("theme_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(d => d.SecondaryThemeColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("secondary_theme_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(d => d.BodyBackgroundColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("body_background_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(d => d.FontFamily)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("font_family");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasMany(d => d.Vehicles)
            .WithOne(v => v.Dealership)
            .HasForeignKey(v => v.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Leads)
            .WithOne(l => l.Dealership)
            .HasForeignKey(l => l.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Users)
            .WithOne(u => u.Dealership)
            .HasForeignKey(u => u.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**File:** `JealPrototype.Infrastructure/Persistence/Configurations/VehicleConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicle");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");

        builder.Property(v => v.DealershipId)
            .IsRequired()
            .HasColumnName("dealership_id");

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("make");

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("model");

        builder.Property(v => v.Year)
            .IsRequired()
            .HasColumnName("year");

        builder.Property(v => v.Price)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasColumnName("price");

        builder.Property(v => v.Mileage)
            .IsRequired()
            .HasColumnName("mileage");

        builder.Property(v => v.Condition)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("condition")
            .HasConversion(
                c => c.ToString().ToLower(),
                c => Enum.Parse<VehicleCondition>(c, true));

        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("status")
            .HasConversion(
                s => s.ToString().ToLower(),
                s => Enum.Parse<VehicleStatus>(s, true));

        builder.Property(v => v.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("title");

        builder.Property(v => v.Description)
            .HasColumnName("description");

        // JSONB mapping for images array
        builder.Property(v => v.Images)
            .HasColumnName("images")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(v => v.DealershipId)
            .HasDatabaseName("idx_vehicle_dealership_id");

        builder.HasIndex(v => v.Status)
            .HasDatabaseName("idx_vehicle_status");

        builder.HasIndex(v => new { v.DealershipId, v.Status })
            .HasDatabaseName("idx_vehicle_dealership_status");
    }
}
```

Continue this pattern for all entities (Lead, User, BlogPost, etc.)

### 3. Create Repository Implementations

**File:** `JealPrototype.Infrastructure/Persistence/Repositories/DealershipRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class DealershipRepository : IDealershipRepository
{
    private readonly ApplicationDbContext _context;

    public DealershipRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dealership?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Dealerships.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Dealership>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dealerships.ToListAsync(cancellationToken);
    }

    public async Task<Dealership> AddAsync(Dealership entity, CancellationToken cancellationToken = default)
    {
        await _context.Dealerships.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Dealership entity, CancellationToken cancellationToken = default)
    {
        _context.Dealerships.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Dealership entity, CancellationToken cancellationToken = default)
    {
        _context.Dealerships.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dealership?> GetByWebsiteUrlAsync(string websiteUrl, CancellationToken cancellationToken = default)
    {
        return await _context.Dealerships
            .FirstOrDefaultAsync(d => d.WebsiteUrl == websiteUrl, cancellationToken);
    }
}
```

---

## API Layer Implementation

### 1. Create Controllers

**File:** `JealPrototype.API/Controllers/DealershipsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JealPrototype.Application.UseCases.Dealerships;
using JealPrototype.Application.DTOs.Dealership;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/dealers")]
public class DealershipsController : ControllerBase
{
    private readonly GetAllDealershipsUseCase _getAllUseCase;
    private readonly GetDealershipByIdUseCase _getByIdUseCase;
    private readonly CreateDealershipUseCase _createUseCase;

    public DealershipsController(
        GetAllDealershipsUseCase getAllUseCase,
        GetDealershipByIdUseCase getByIdUseCase,
        CreateDealershipUseCase createUseCase)
    {
        _getAllUseCase = getAllUseCase;
        _getByIdUseCase = getByIdUseCase;
        _createUseCase = createUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var dealerships = await _getAllUseCase.ExecuteAsync(cancellationToken);
        return Ok(dealerships);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var dealership = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        if (dealership == null)
            return NotFound(new { error = "Dealership not found" });

        return Ok(dealership);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateDealershipDto dto, CancellationToken cancellationToken)
    {
        var dealership = await _createUseCase.ExecuteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dealership.Id }, dealership);
    }
}
```

### 2. Create Authentication Service

**File:** `JealPrototype.Infrastructure/Services/JwtAuthService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Infrastructure.Services;

public class JwtAuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public JwtAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("user_type", user.UserType.ToString())
        };

        if (user.DealershipId.HasValue)
            claims.Add(new Claim("dealership_id", user.DealershipId.Value.ToString()));

        foreach (var permission in user.Permissions)
            claims.Add(new Claim("permission", permission.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
```

### 3. Create Program.cs

**File:** `JealPrototype.API/Program.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using JealPrototype.Infrastructure.Persistence;
using JealPrototype.Application.Mapping;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateDealershipValidator>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register Use Cases, Repositories, Services
// ... (register all dependencies)

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Testing Strategy

### Unit Tests

Create unit tests for:
- Domain entities (business logic)
- Value objects (validation)
- Use cases (application logic)

### Integration Tests

Create integration tests for:
- API endpoints (full request/response cycle)
- Database operations (repository layer)
- External services (mocked)

---

**End of Implementation Guide**

**Next Steps:**
1. Install .NET 8.0 SDK
2. Run solution setup commands
3. Implement entities and value objects
4. Set up database connection to existing PostgreSQL
5. Implement repositories and use cases
6. Create API controllers
7. Test with existing frontend

