namespace JealPrototype.Domain.Enums;

/// <summary>
/// Status of an EasyCars stock synchronization operation
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// All items processed successfully without errors
    /// </summary>
    Success = 0,

    /// <summary>
    /// Some items succeeded and some failed
    /// </summary>
    PartialSuccess = 1,

    /// <summary>
    /// All items failed or sync operation failed completely
    /// </summary>
    Failed = 2
}
