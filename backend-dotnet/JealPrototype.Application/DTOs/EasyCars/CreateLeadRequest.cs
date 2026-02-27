namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Request model for creating a new lead in EasyCars API
/// </summary>
public class CreateLeadRequest
{
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
}
