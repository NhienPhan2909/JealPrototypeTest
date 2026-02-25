namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response from EasyCars GetAdvertisementStocks API
/// </summary>
public class StockResponse : EasyCarsBaseResponse
{
    /// <summary>
    /// List of stock items returned from the API
    /// </summary>
    public List<StockItem>? Stocks { get; set; }

    /// <summary>
    /// Total number of records (if pagination is supported)
    /// </summary>
    public int? TotalRecords { get; set; }

    /// <summary>
    /// Current page number (if pagination is supported)
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Page size (if pagination is supported)
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Total number of pages (if pagination is supported)
    /// </summary>
    public int? TotalPages { get; set; }
}
