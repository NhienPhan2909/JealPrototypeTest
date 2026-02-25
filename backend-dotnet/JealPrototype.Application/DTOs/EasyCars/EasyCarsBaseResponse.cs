namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Base class for all EasyCars API responses
/// All EasyCars responses include ResponseCode and ResponseMessage
/// </summary>
public abstract class EasyCarsBaseResponse
{
    /// <summary>
    /// Response code from EasyCars API (mapped from "ResponseCode" field)
    /// 0 = Success, 1 = Auth failure, 5 = Temporary error, 7 = Validation error, 9 = Fatal error
    /// </summary>
    public int ResponseCode { get; set; }

    /// <summary>
    /// Response code from EasyCars API (mapped from "Code" field — used in error responses)
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Response message from EasyCars API
    /// </summary>
    public string ResponseMessage { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the response was successful (both ResponseCode and Code are 0)
    /// </summary>
    public bool IsSuccess => ResponseCode == 0 && Code == 0;
}
