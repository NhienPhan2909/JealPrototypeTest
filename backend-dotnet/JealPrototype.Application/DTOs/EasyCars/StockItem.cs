using System.Text.Json.Serialization;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Represents a stock item from EasyCars GetAdvertisementStocks API
/// Contains 70+ fields covering vehicle details, pricing, features, and metadata
/// </summary>
public class StockItem
{
    // ========== Primary Identification (5 fields) ==========
    
    /// <summary>
    /// Unique stock number assigned by dealership
    /// </summary>
    public string StockNumber { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle Identification Number (17 characters)
    /// </summary>
    public string VIN { get; set; } = string.Empty;

    /// <summary>
    /// Registration/License plate number
    /// </summary>
    public string RegoNum { get; set; } = string.Empty;

    /// <summary>
    /// Yard code identifying physical location
    /// </summary>
    public string YardCode { get; set; } = string.Empty;

    /// <summary>
    /// Stock type: "Used", "New", "Demo"
    /// </summary>
    public string StockType { get; set; } = string.Empty;

    // ========== Vehicle Details (15 fields) ==========

    /// <summary>
    /// Vehicle manufacturer (e.g., "Honda", "Toyota")
    /// </summary>
    public string Make { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle model (e.g., "Accord", "Camry")
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Model badge/trim level (e.g., "VTi-L", "Sport")
    /// </summary>
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturing year
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Body type (e.g., "Sedan", "SUV", "Hatchback")
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Exterior color
    /// </summary>
    public string Colour { get; set; } = string.Empty;

    /// <summary>
    /// Number of doors
    /// </summary>
    public int Doors { get; set; }

    /// <summary>
    /// Seating capacity
    /// </summary>
    public int Seats { get; set; }

    /// <summary>
    /// Transmission type (e.g., "Automatic", "Manual")
    /// </summary>
    public string Transmission { get; set; } = string.Empty;

    /// <summary>
    /// Fuel type (e.g., "Petrol", "Diesel", "Hybrid")
    /// </summary>
    public string FuelType { get; set; } = string.Empty;

    /// <summary>
    /// Engine size (e.g., "2.4L", "3.5L")
    /// </summary>
    public string EngineSize { get; set; } = string.Empty;

    /// <summary>
    /// Number of cylinders
    /// </summary>
    public int Cylinders { get; set; }

    /// <summary>
    /// Drive type (e.g., "FWD", "RWD", "AWD", "4WD")
    /// </summary>
    public string DriveType { get; set; } = string.Empty;

    /// <summary>
    /// Odometer reading in kilometers
    /// </summary>
    public int Odometer { get; set; }

    /// <summary>
    /// Registration expiry date
    /// </summary>
    public string RegistrationExpiry { get; set; } = string.Empty;

    // ========== Pricing & Financial (8 fields) ==========

    /// <summary>
    /// Listed price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Cost price (dealer's cost)
    /// </summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Recommended retail price
    /// </summary>
    public decimal? RetailPrice { get; set; }

    /// <summary>
    /// Trade-in value if applicable
    /// </summary>
    public decimal? TradeInValue { get; set; }

    /// <summary>
    /// Weekly payment cost for financing
    /// </summary>
    public decimal? WeeklyCost { get; set; }

    /// <summary>
    /// Price type (e.g., "Drive Away", "Excl. Govt Charges")
    /// </summary>
    public string PriceType { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if price is negotiable
    /// </summary>
    public bool NegotiablePrice { get; set; }

    /// <summary>
    /// Indicates if tax is included in price
    /// </summary>
    public bool TaxIncluded { get; set; }

    // ========== Marketing & Description (10 fields) ==========

    /// <summary>
    /// Main vehicle description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional features description
    /// </summary>
    public string AdditionalFeatures { get; set; } = string.Empty;

    /// <summary>
    /// Key selling features
    /// </summary>
    public string KeyFeatures { get; set; } = string.Empty;

    /// <summary>
    /// Additional comments or notes
    /// </summary>
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this is a featured vehicle
    /// </summary>
    public bool FeaturedVehicle { get; set; }

    /// <summary>
    /// Display priority (higher = more prominent)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Current status (e.g., "Available", "Sold", "Reserved")
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Condition (e.g., "Excellent", "Good", "Fair")
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Category classification
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Tags for categorization and search
    /// </summary>
    public List<string> Tags { get; set; } = new();

    // ========== Images & Media (5 fields) ==========

    /// <summary>
    /// List of image URLs
    /// </summary>
    public List<string> ImageURLs { get; set; } = new();

    /// <summary>
    /// Thumbnail image URL
    /// </summary>
    public string ThumbnailURL { get; set; } = string.Empty;

    /// <summary>
    /// Video URL if available
    /// </summary>
    public string VideoURL { get; set; } = string.Empty;

    /// <summary>
    /// Virtual tour URL if available
    /// </summary>
    public string VirtualTourURL { get; set; } = string.Empty;

    /// <summary>
    /// Total number of images
    /// </summary>
    public int ImageCount { get; set; }

    // ========== Timestamps & Audit (5 fields) ==========

    /// <summary>
    /// Date vehicle was added to inventory
    /// </summary>
    public DateTime DateAdded { get; set; }

    /// <summary>
    /// Date vehicle information was last updated
    /// </summary>
    public DateTime DateUpdated { get; set; }

    /// <summary>
    /// Date vehicle was sold (if applicable)
    /// </summary>
    public DateTime? DateSold { get; set; }

    /// <summary>
    /// Date vehicle was received by dealership
    /// </summary>
    public DateTime? DateReceived { get; set; }

    /// <summary>
    /// Number of days vehicle has been in stock
    /// </summary>
    public int DaysInStock { get; set; }

    // ========== Optional Features & Accessories (10 fields) ==========

    /// <summary>
    /// Air conditioning available
    /// </summary>
    public bool AirConditioning { get; set; }

    /// <summary>
    /// Cruise control available
    /// </summary>
    public bool CruiseControl { get; set; }

    /// <summary>
    /// Power steering available
    /// </summary>
    public bool PowerSteering { get; set; }

    /// <summary>
    /// Power windows available
    /// </summary>
    public bool PowerWindows { get; set; }

    /// <summary>
    /// Power locks available
    /// </summary>
    public bool PowerLocks { get; set; }

    /// <summary>
    /// Anti-lock braking system (ABS) available
    /// </summary>
    public bool ABS { get; set; }

    /// <summary>
    /// Airbags available
    /// </summary>
    public bool Airbags { get; set; }

    /// <summary>
    /// Alloy wheels available
    /// </summary>
    public bool AlloyWheels { get; set; }

    /// <summary>
    /// Satellite navigation available
    /// </summary>
    public bool SatNav { get; set; }

    /// <summary>
    /// Sunroof available
    /// </summary>
    public bool Sunroof { get; set; }

    // ========== Dealer/Location Info (5 fields) ==========

    /// <summary>
    /// Dealership name
    /// </summary>
    public string DealerName { get; set; } = string.Empty;

    /// <summary>
    /// Location city
    /// </summary>
    public string LocationCity { get; set; } = string.Empty;

    /// <summary>
    /// Location state/province
    /// </summary>
    public string LocationState { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    // ========== Additional Fields (7 fields) ==========

    /// <summary>
    /// Warranty information
    /// </summary>
    public string? WarrantyInfo { get; set; }

    /// <summary>
    /// Service history details
    /// </summary>
    public string? ServiceHistory { get; set; }

    /// <summary>
    /// Number of previous owners
    /// </summary>
    public int? PreviousOwners { get; set; }

    /// <summary>
    /// Compliance date
    /// </summary>
    public string? ComplianceDate { get; set; }

    /// <summary>
    /// Build date
    /// </summary>
    public string? BuildDate { get; set; }

    /// <summary>
    /// Additional unmapped data from API
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}
