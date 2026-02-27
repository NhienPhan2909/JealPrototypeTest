namespace JealPrototype.Application.DTOs.EasyCars;

public class ResolveConflictRequest
{
    /// <summary>Must be "local" or "remote"</summary>
    public string Resolution { get; set; } = string.Empty;
}
