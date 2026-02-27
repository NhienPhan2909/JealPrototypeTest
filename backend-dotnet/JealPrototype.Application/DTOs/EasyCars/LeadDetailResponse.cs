namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response model for GetLeadDetail EasyCars API endpoint
/// </summary>
public class LeadDetailResponse : EasyCarsBaseResponse
{
    public string? LeadNumber { get; set; }
    public string? CustomerNo { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerMobile { get; set; }
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
    public int? LeadStatus { get; set; }
    public string? CreatedDate { get; set; }
    public string? UpdatedDate { get; set; }
}
