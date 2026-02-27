namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response model for CreateLead EasyCars API endpoint
/// </summary>
public class CreateLeadResponse : EasyCarsBaseResponse
{
    public string? LeadNumber { get; set; }
    public string? CustomerNo { get; set; }
}
