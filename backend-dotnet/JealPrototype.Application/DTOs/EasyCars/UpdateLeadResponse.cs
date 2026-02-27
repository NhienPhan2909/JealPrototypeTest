namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response model for UpdateLead EasyCars API endpoint
/// </summary>
public class UpdateLeadResponse : EasyCarsBaseResponse
{
    public string? LeadNumber { get; set; }
}
