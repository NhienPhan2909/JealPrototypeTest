using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Entities;

public class Lead : BaseEntity
{
    public int DealershipId { get; private set; }
    public int? VehicleId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public LeadStatus Status { get; private set; } = LeadStatus.Received;

    public Dealership Dealership { get; private set; } = null!;
    public Vehicle? Vehicle { get; private set; }

    private Lead() { }

    public static Lead Create(
        int dealershipId,
        string name,
        string email,
        string phone,
        string message,
        int? vehicleId = null)
    {
        if (dealershipId <= 0)
            throw new ArgumentException("Invalid dealership ID", nameof(dealershipId));

        if (string.IsNullOrWhiteSpace(name) || name.Length > 255)
            throw new ArgumentException("Name is required and must be 255 characters or less", nameof(name));

        if (string.IsNullOrWhiteSpace(email) || email.Length > 255)
            throw new ArgumentException("Email is required and must be 255 characters or less", nameof(email));

        if (string.IsNullOrWhiteSpace(phone) || phone.Length > 20)
            throw new ArgumentException("Phone is required and must be 20 characters or less", nameof(phone));

        if (string.IsNullOrWhiteSpace(message) || message.Length > 5000)
            throw new ArgumentException("Message is required and must be 5000 characters or less", nameof(message));

        return new Lead
        {
            DealershipId = dealershipId,
            VehicleId = vehicleId,
            Name = name,
            Email = email,
            Phone = phone,
            Message = message
        };
    }

    public void UpdateStatus(LeadStatus newStatus)
    {
        Status = newStatus;
    }
}
