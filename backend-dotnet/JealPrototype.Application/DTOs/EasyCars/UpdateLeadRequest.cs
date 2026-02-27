namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Request model for updating an existing lead in EasyCars API
/// </summary>
public class UpdateLeadRequest
{
    public string LeadNumber { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountSecret { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerMobile { get; set; }
    public string? CustomerNo { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public decimal? VehiclePrice { get; set; }
    public int? VehicleType { get; set; }
    public int? VehicleInterest { get; set; }
    public int? FinanceStatus { get; set; }
    public int? Rating { get; set; }
    public string? StockNumber { get; set; }
    public string? Comments { get; set; }
    /// <summary>EasyCars LeadStatus integer: 10=New, 30=InProgress, 50=Won, 60=Lost, 90=Deleted. Null = no status change.</summary>
    public int? LeadStatus { get; set; }
}
