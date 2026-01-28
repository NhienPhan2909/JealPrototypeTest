using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Entities;

public class SalesRequest : BaseEntity
{
    public int DealershipId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Make { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int Year { get; private set; }
    public int Kilometers { get; private set; }
    public string? AdditionalMessage { get; private set; }
    public LeadStatus Status { get; private set; } = LeadStatus.Received;

    public Dealership Dealership { get; private set; } = null!;

    private SalesRequest() { }

    public static SalesRequest Create(
        int dealershipId,
        string name,
        string email,
        string phone,
        string make,
        string model,
        int year,
        int kilometers,
        string? additionalMessage = null)
    {
        if (dealershipId <= 0)
            throw new ArgumentException("Invalid dealership ID", nameof(dealershipId));

        if (string.IsNullOrWhiteSpace(name) || name.Length > 255)
            throw new ArgumentException("Name is required and must be 255 characters or less", nameof(name));

        if (string.IsNullOrWhiteSpace(email) || email.Length > 255)
            throw new ArgumentException("Email is required and must be 255 characters or less", nameof(email));

        if (string.IsNullOrWhiteSpace(phone) || phone.Length > 20)
            throw new ArgumentException("Phone is required and must be 20 characters or less", nameof(phone));

        if (string.IsNullOrWhiteSpace(make) || make.Length > 100)
            throw new ArgumentException("Make is required and must be 100 characters or less", nameof(make));

        if (string.IsNullOrWhiteSpace(model) || model.Length > 100)
            throw new ArgumentException("Model is required and must be 100 characters or less", nameof(model));

        if (year < 1900 || year > DateTime.Now.Year + 1)
            throw new ArgumentException("Invalid year", nameof(year));

        if (kilometers < 0)
            throw new ArgumentException("Kilometers must be non-negative", nameof(kilometers));

        return new SalesRequest
        {
            DealershipId = dealershipId,
            Name = name,
            Email = email,
            Phone = phone,
            Make = make,
            Model = model,
            Year = year,
            Kilometers = kilometers,
            AdditionalMessage = additionalMessage
        };
    }

    public void UpdateStatus(LeadStatus newStatus)
    {
        Status = newStatus;
    }
}
