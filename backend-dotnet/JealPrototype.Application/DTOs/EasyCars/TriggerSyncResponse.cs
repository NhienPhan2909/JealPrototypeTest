namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response for manual sync trigger endpoint
/// </summary>
public class TriggerSyncResponse
{
    public string Message { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
}
