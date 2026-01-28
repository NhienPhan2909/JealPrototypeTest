using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Entities;

public class Vehicle : BaseEntity
{
    public int DealershipId { get; private set; }
    public string Make { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int Year { get; private set; }
    public decimal Price { get; private set; }
    public int Mileage { get; private set; }
    public VehicleCondition Condition { get; private set; }
    public VehicleStatus Status { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public List<string> Images { get; private set; } = new();

    public Dealership Dealership { get; private set; } = null!;

    private Vehicle() { }
    
    // Public getters for backward compatibility with Node.js API response format
    public string GetExteriorColor() => ""; // Not in DB schema
    public string GetInteriorColor() => ""; // Not in DB schema
    public string GetVin() => ""; // Not in DB schema
    public string GetStockNumber() => ""; // Not in DB schema
    public string GetFeatures() => ""; // Not in DB schema
    public List<string> GetImageUrls() => Images;
    public bool GetIsFeatured() => false; // Not in DB schema
    public bool GetIsAvailable() => Status == VehicleStatus.Active;

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
