using System.Text.Json;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.Services.EasyCars;

/// <summary>
/// Pure mapper service for converting between Lead domain entities and EasyCars API DTOs.
/// </summary>
public class EasyCarsLeadMapper : IEasyCarsLeadMapper
{
    private readonly ILogger<EasyCarsLeadMapper> _logger;

    public EasyCarsLeadMapper(ILogger<EasyCarsLeadMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public CreateLeadRequest MapToCreateLeadRequest(Lead lead, string accountNumber, string accountSecret, Vehicle? vehicle)
    {
        var request = new CreateLeadRequest
        {
            AccountNumber = accountNumber,
            AccountSecret = accountSecret,
            CustomerName = lead.Name,
            CustomerEmail = lead.Email,
            CustomerPhone = lead.Phone,
            CustomerNo = lead.EasyCarsCustomerNo,
            Comments = lead.Message,
            VehicleInterest = MapVehicleInterestTypeToInt(lead.VehicleInterestType),
            FinanceStatus = MapFinanceStatusToInt(lead.FinanceInterested),
            Rating = MapRatingToInt(lead.Rating)
        };

        if (vehicle != null)
        {
            request.VehicleMake = vehicle.Make;
            request.VehicleModel = vehicle.Model;
            request.VehicleYear = vehicle.Year;
            request.VehiclePrice = vehicle.Price;
            request.StockNumber = vehicle.EasyCarsStockNumber;
        }

        return request;
    }

    public UpdateLeadRequest MapToUpdateLeadRequest(Lead lead, string leadNumber, string accountNumber, string accountSecret, Vehicle? vehicle)
    {
        var request = new UpdateLeadRequest
        {
            LeadNumber = leadNumber,
            AccountNumber = accountNumber,
            AccountSecret = accountSecret,
            CustomerName = lead.Name,
            CustomerEmail = lead.Email,
            CustomerPhone = lead.Phone,
            CustomerNo = lead.EasyCarsCustomerNo,
            Comments = lead.Message,
            VehicleInterest = MapVehicleInterestTypeToInt(lead.VehicleInterestType),
            FinanceStatus = MapFinanceStatusToInt(lead.FinanceInterested),
            Rating = MapRatingToInt(lead.Rating)
        };

        if (vehicle != null)
        {
            request.VehicleMake = vehicle.Make;
            request.VehicleModel = vehicle.Model;
            request.VehicleYear = vehicle.Year;
            request.VehiclePrice = vehicle.Price;
            request.StockNumber = vehicle.EasyCarsStockNumber;
        }

        return request;
    }

    public Lead MapFromEasyCarsLead(LeadDetailResponse response, int dealershipId)
    {
        var name = Truncate(response.CustomerName ?? "Unknown", 255);
        var email = Truncate(response.CustomerEmail ?? string.Empty, 255);
        var phone = Truncate(response.CustomerPhone ?? response.CustomerMobile ?? string.Empty, 20);
        var message = Truncate(response.Comments ?? string.Empty, 5000);

        // Lead.Create validates non-empty for name, email, phone, message
        if (string.IsNullOrWhiteSpace(email)) email = "unknown@easycars.com";
        if (string.IsNullOrWhiteSpace(phone)) phone = "0000000000";
        if (string.IsNullOrWhiteSpace(message)) message = "Imported from EasyCars";

        var lead = Lead.Create(dealershipId, name, email, phone, message);

        var rawJson = JsonSerializer.Serialize(response);
        var vehicleInterestType = MapIntToVehicleInterestType(response.VehicleInterest);
        var financeInterested = response.FinanceStatus == 1;
        var rating = MapIntToRating(response.Rating);

        lead.UpdateEasyCarsData(response.LeadNumber, response.CustomerNo, rawJson, vehicleInterestType, financeInterested, rating);
        lead.UpdateStatus(MapLeadStatusFromEasyCars(response.LeadStatus));

        _logger.LogDebug("Mapped EasyCars lead {LeadNumber} to Lead entity for dealership {DealershipId}",
            response.LeadNumber, dealershipId);

        return lead;
    }

    public void UpdateLeadFromResponse(Lead lead, LeadDetailResponse response)
    {
        var rawJson = JsonSerializer.Serialize(response);
        var vehicleInterestType = MapIntToVehicleInterestType(response.VehicleInterest);
        var financeInterested = response.FinanceStatus == 1;
        var rating = MapIntToRating(response.Rating);

        lead.UpdateEasyCarsData(response.LeadNumber, response.CustomerNo, rawJson, vehicleInterestType, financeInterested, rating);
        lead.MarkSyncedFromEasyCars(DateTime.UtcNow);
    }

    public bool IsExistingLead(Lead lead, LeadDetailResponse response)
    {
        return !string.IsNullOrEmpty(lead.EasyCarsLeadNumber) && lead.EasyCarsLeadNumber == response.LeadNumber;
    }

    // --- Private helpers ---

    private static int? MapVehicleInterestTypeToInt(string? interestType) => interestType switch
    {
        "Purchase" => 1,
        "Finance" => 2,
        "TradeIn" => 3,
        "ServiceRepair" => 4,
        "Other" => 5,
        _ => null
    };

    private static string? MapIntToVehicleInterestType(int? value) => value switch
    {
        1 => "Purchase",
        2 => "Finance",
        3 => "TradeIn",
        4 => "ServiceRepair",
        5 => "Other",
        _ => null
    };

    private static int? MapRatingToInt(string? rating) => rating switch
    {
        "Hot" => 1,
        "Warm" => 2,
        "Cold" => 3,
        _ => null
    };

    private static string? MapIntToRating(int? value) => value switch
    {
        1 => "Hot",
        2 => "Warm",
        3 => "Cold",
        _ => null
    };

    private static LeadStatus MapLeadStatusFromEasyCars(int? easyCarsStatus) => easyCarsStatus switch
    {
        10 => LeadStatus.Received,
        30 => LeadStatus.InProgress,
        50 => LeadStatus.Won,
        60 => LeadStatus.Lost,
        90 => LeadStatus.Deleted,
        _  => LeadStatus.Received
    };

    private static int? MapFinanceStatusToInt(bool financeInterested) =>
        financeInterested ? 1 : null;

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    public int MapLeadStatusToEasyCars(LeadStatus status) => status switch
    {
        LeadStatus.Received   => 10,
        LeadStatus.InProgress => 30,
        LeadStatus.Won        => 50,
        LeadStatus.Done       => 50,  // legacy — maps to Won
        LeadStatus.Lost       => 60,
        LeadStatus.Deleted    => 90,
        _                     => 10
    };

    public LeadStatus MapLeadStatusFromInt(int easyCarsStatus) =>
        MapLeadStatusFromEasyCars(easyCarsStatus);

    public UpdateLeadRequest MapToStatusOnlyUpdateRequest(Lead lead, string accountNumber, string accountSecret)
    {
        return new UpdateLeadRequest
        {
            LeadNumber    = lead.EasyCarsLeadNumber!,
            AccountNumber = accountNumber,
            AccountSecret = accountSecret,
            CustomerName  = lead.Name,
            CustomerEmail = lead.Email,
            LeadStatus    = MapLeadStatusToEasyCars(lead.Status)
        };
    }
}
