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

    // EasyCars Integration Fields
    public string? EasyCarsLeadNumber { get; private set; }
    public string? EasyCarsCustomerNo { get; private set; }
    public string? EasyCarsRawData { get; private set; }
    public DataSource DataSource { get; private set; } = DataSource.Manual;
    public DateTime? LastSyncedToEasyCars { get; private set; }
    public DateTime? LastSyncedFromEasyCars { get; private set; }
    public string? VehicleInterestType { get; private set; }
    public bool FinanceInterested { get; private set; } = false;
    public string? Rating { get; private set; }

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

    public void UpdateEasyCarsData(
        string? leadNumber,
        string? customerNo,
        string? rawData,
        string? vehicleInterestType = null,
        bool financeInterested = false,
        string? rating = null)
    {
        EasyCarsLeadNumber = leadNumber;
        EasyCarsCustomerNo = customerNo;
        EasyCarsRawData = rawData;
        VehicleInterestType = vehicleInterestType;
        FinanceInterested = financeInterested;
        Rating = rating;
        DataSource = DataSource.EasyCars;
    }

    public void SetEasyCarsData(
        string leadNumber,
        string customerNo,
        string rawData)
    {
        EasyCarsLeadNumber = leadNumber;
        EasyCarsCustomerNo = customerNo;
        EasyCarsRawData = rawData;
        DataSource = DataSource.EasyCars;
        LastSyncedFromEasyCars = DateTime.UtcNow;
    }

    public void MarkSyncedToEasyCars(DateTime syncTime) => LastSyncedToEasyCars = syncTime;

    public void MarkSyncedFromEasyCars(DateTime syncTime) => LastSyncedFromEasyCars = syncTime;
}
