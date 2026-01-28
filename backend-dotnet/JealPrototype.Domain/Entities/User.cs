using JealPrototype.Domain.Enums;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public UserType UserType { get; private set; }
    public int? DealershipId { get; private set; }
    public List<Permission> Permissions { get; private set; } = new();
    public int? CreatedBy { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Dealership? Dealership { get; private set; }

    private User() { }
    
    // Public getters for backward compatibility with Node.js API
    public string GetFirstName() => FullName.Split(' ').FirstOrDefault() ?? "";
    public string GetLastName() => FullName.Split(' ').Skip(1).FirstOrDefault() ?? "";
    public string GetRole() => UserType.ToString().ToLower().Replace("dealership", "dealership_");
    public int? GetCreatedById() => CreatedBy;

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
            return true;

        return Permissions.Contains(permission);
    }
}
