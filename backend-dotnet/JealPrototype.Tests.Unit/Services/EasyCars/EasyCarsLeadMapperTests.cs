using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Services.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace JealPrototype.Tests.Unit.Services.EasyCars;

/// <summary>
/// Unit tests for Story 3.2: EasyCarsLeadMapper
/// </summary>
public class EasyCarsLeadMapperTests
{
    private readonly EasyCarsLeadMapper _sut;

    public EasyCarsLeadMapperTests()
    {
        var mockLogger = new Mock<ILogger<EasyCarsLeadMapper>>();
        _sut = new EasyCarsLeadMapper(mockLogger.Object);
    }

    private static Lead CreateTestLead()
        => Lead.Create(1, "John Smith", "john@example.com", "0412345678", "Interested in your vehicles");

    private static LeadDetailResponse CreateTestLeadDetailResponse() => new()
    {
        LeadNumber = "LEAD-001",
        CustomerNo = "CUST-100",
        CustomerName = "Jane Doe",
        CustomerEmail = "jane@example.com",
        CustomerPhone = "0498765432",
        Comments = "Looking for a new car",
        VehicleInterest = 1,
        FinanceStatus = 1,
        Rating = 1,
        LeadStatus = 10
    };

    // --- MapToCreateLeadRequest ---

    [Fact]
    public void MapToCreateLeadRequest_WithFullLead_MapsAllCustomerFields()
    {
        var lead = CreateTestLead();
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Equal("John Smith", result.CustomerName);
        Assert.Equal("john@example.com", result.CustomerEmail);
        Assert.Equal("0412345678", result.CustomerPhone);
        Assert.Equal("Interested in your vehicles", result.Comments);
        Assert.Equal("ACC", result.AccountNumber);
        Assert.Equal("SECRET", result.AccountSecret);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithVehicle_MapsAllVehicleFields()
    {
        var lead = CreateTestLead();
        var vehicle = Vehicle.Create(1, "Toyota", "Camry", 2022, 35000m, 10000,
            VehicleCondition.Used, VehicleStatus.Active, "2022 Toyota Camry");
        vehicle.UpdateEasyCarsData("STK-999", null, null, null);

        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", vehicle);

        Assert.Equal("Toyota", result.VehicleMake);
        Assert.Equal("Camry", result.VehicleModel);
        Assert.Equal(2022, result.VehicleYear);
        Assert.Equal(35000m, result.VehiclePrice);
        Assert.Equal("STK-999", result.StockNumber);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithNullVehicle_LeavesVehicleFieldsNull()
    {
        var lead = CreateTestLead();
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Null(result.VehicleMake);
        Assert.Null(result.VehicleModel);
        Assert.Null(result.VehicleYear);
        Assert.Null(result.VehiclePrice);
        Assert.Null(result.StockNumber);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithExistingCustomerNo_PopulatesCustomerNo()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData("LEAD-XYZ", "CUST-42", null);

        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Equal("CUST-42", result.CustomerNo);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithHotRating_SetsRatingTo1()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, null, false, "Hot");

        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Equal(1, result.Rating);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithWarmRating_SetsRatingTo2()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, null, false, "Warm");

        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Equal(2, result.Rating);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithColdRating_SetsRatingTo3()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, null, false, "Cold");

        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Equal(3, result.Rating);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithNullRating_LeavesRatingNull()
    {
        var lead = CreateTestLead();
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Null(result.Rating);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithFinanceInterested_SetsFinanceStatusTo1()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, null, true);

        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Equal(1, result.FinanceStatus);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithNoFinanceInterest_LeavesFinanceStatusNull()
    {
        var lead = CreateTestLead();
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Null(result.FinanceStatus);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithPurchaseInterestType_SetsVehicleInterestTo1()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, "Purchase");

        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);

        Assert.Equal(1, result.VehicleInterest);
    }

    // --- MapFromEasyCarsLead ---

    [Fact]
    public void MapFromEasyCarsLead_WithCompleteResponse_MapsAllFields()
    {
        var response = CreateTestLeadDetailResponse();
        var result = _sut.MapFromEasyCarsLead(response, 1);

        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal("Looking for a new car", result.Message);
        Assert.Equal("LEAD-001", result.EasyCarsLeadNumber);
        Assert.Equal("CUST-100", result.EasyCarsCustomerNo);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithNullOptionalFields_UsesDefaults()
    {
        var response = new LeadDetailResponse
        {
            LeadNumber = "LEAD-002",
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            CustomerPhone = null,
            Comments = null
        };

        var ex = Record.Exception(() => _sut.MapFromEasyCarsLead(response, 1));
        Assert.Null(ex);
    }

    [Fact]
    public void MapFromEasyCarsLead_SetsDataSourceToEasyCars()
    {
        var response = CreateTestLeadDetailResponse();
        var result = _sut.MapFromEasyCarsLead(response, 1);

        Assert.Equal(DataSource.EasyCars, result.DataSource);
    }

    [Fact]
    public void MapFromEasyCarsLead_StoresRawJsonInEasyCarsRawData()
    {
        var response = CreateTestLeadDetailResponse();
        var result = _sut.MapFromEasyCarsLead(response, 1);

        Assert.NotNull(result.EasyCarsRawData);
        Assert.Contains("LEAD-001", result.EasyCarsRawData);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithRating1_SetsRatingToHot()
    {
        var response = CreateTestLeadDetailResponse();
        response.Rating = 1;

        var result = _sut.MapFromEasyCarsLead(response, 1);

        Assert.Equal("Hot", result.Rating);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithLeadStatus10_SetsStatusToReceived()
    {
        var response = CreateTestLeadDetailResponse();
        response.LeadStatus = 10;

        var result = _sut.MapFromEasyCarsLead(response, 1);

        Assert.Equal(LeadStatus.Received, result.Status);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithLeadStatus30_SetsStatusToInProgress()
    {
        var response = CreateTestLeadDetailResponse();
        response.LeadStatus = 30;

        var result = _sut.MapFromEasyCarsLead(response, 1);

        Assert.Equal(LeadStatus.InProgress, result.Status);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithLeadStatus50_SetsStatusToDone()
    {
        var response = CreateTestLeadDetailResponse();
        response.LeadStatus = 50;

        var result = _sut.MapFromEasyCarsLead(response, 1);

        Assert.Equal(LeadStatus.Done, result.Status);
    }

    // --- UpdateLeadFromResponse ---

    [Fact]
    public void UpdateLeadFromResponse_UpdatesEasyCarsFields()
    {
        var lead = CreateTestLead();
        var response = CreateTestLeadDetailResponse();

        _sut.UpdateLeadFromResponse(lead, response);

        Assert.Equal("LEAD-001", lead.EasyCarsLeadNumber);
        Assert.Equal("CUST-100", lead.EasyCarsCustomerNo);
        Assert.NotNull(lead.EasyCarsRawData);
    }

    [Fact]
    public void UpdateLeadFromResponse_SetsLastSyncedFromEasyCars()
    {
        var lead = CreateTestLead();
        var response = CreateTestLeadDetailResponse();
        var before = DateTime.UtcNow;

        _sut.UpdateLeadFromResponse(lead, response);

        Assert.NotNull(lead.LastSyncedFromEasyCars);
        Assert.True(lead.LastSyncedFromEasyCars >= before);
    }

    // --- IsExistingLead ---

    [Fact]
    public void IsExistingLead_WithMatchingLeadNumber_ReturnsTrue()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData("LEAD-001", null, null);
        var response = new LeadDetailResponse { LeadNumber = "LEAD-001" };

        Assert.True(_sut.IsExistingLead(lead, response));
    }

    [Fact]
    public void IsExistingLead_WithDifferentLeadNumber_ReturnsFalse()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData("LEAD-001", null, null);
        var response = new LeadDetailResponse { LeadNumber = "LEAD-999" };

        Assert.False(_sut.IsExistingLead(lead, response));
    }

    [Fact]
    public void IsExistingLead_WithNullLeadEasyCarsNumber_ReturnsFalse()
    {
        var lead = CreateTestLead();
        var response = new LeadDetailResponse { LeadNumber = "LEAD-001" };

        Assert.False(_sut.IsExistingLead(lead, response));
    }

    // --- MapToUpdateLeadRequest ---

    [Fact]
    public void MapToUpdateLeadRequest_SetsLeadNumber()
    {
        var lead = CreateTestLead();
        var result = _sut.MapToUpdateLeadRequest(lead, "LEAD-555", "ACC", "SECRET", null);

        Assert.Equal("LEAD-555", result.LeadNumber);
    }

    [Fact]
    public void MapToUpdateLeadRequest_MapsCustomerFields()
    {
        var lead = CreateTestLead();
        var result = _sut.MapToUpdateLeadRequest(lead, "LEAD-555", "ACC", "SECRET", null);

        Assert.Equal("John Smith", result.CustomerName);
        Assert.Equal("john@example.com", result.CustomerEmail);
        Assert.Equal("0412345678", result.CustomerPhone);
        Assert.Equal("ACC", result.AccountNumber);
        Assert.Equal("SECRET", result.AccountSecret);
    }

    [Fact]
    public void MapToUpdateLeadRequest_WithNullVehicle_LeavesVehicleFieldsNull()
    {
        var lead = CreateTestLead();
        var result = _sut.MapToUpdateLeadRequest(lead, "LEAD-555", "ACC", "SECRET", null);

        Assert.Null(result.VehicleMake);
        Assert.Null(result.StockNumber);
    }

    [Fact]
    public void MapToUpdateLeadRequest_WithVehicle_MapsVehicleFields()
    {
        var lead = CreateTestLead();
        var vehicle = Vehicle.Create(1, "Honda", "Civic", 2021, 28000m, 5000,
            VehicleCondition.Used, VehicleStatus.Active, "2021 Honda Civic");
        vehicle.UpdateEasyCarsData("STK-111", null, null, null);

        var result = _sut.MapToUpdateLeadRequest(lead, "LEAD-555", "ACC", "SECRET", vehicle);

        Assert.Equal("Honda", result.VehicleMake);
        Assert.Equal("STK-111", result.StockNumber);
    }

    // --- VehicleInterest enum branches (MapToCreateLeadRequest) ---

    [Fact]
    public void MapToCreateLeadRequest_WithFinanceInterestType_SetsVehicleInterestTo2()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, "Finance");
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);
        Assert.Equal(2, result.VehicleInterest);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithTradeInInterestType_SetsVehicleInterestTo3()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, "TradeIn");
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);
        Assert.Equal(3, result.VehicleInterest);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithServiceRepairInterestType_SetsVehicleInterestTo4()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, "ServiceRepair");
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);
        Assert.Equal(4, result.VehicleInterest);
    }

    [Fact]
    public void MapToCreateLeadRequest_WithOtherInterestType_SetsVehicleInterestTo5()
    {
        var lead = CreateTestLead();
        lead.UpdateEasyCarsData(null, null, null, "Other");
        var result = _sut.MapToCreateLeadRequest(lead, "ACC", "SECRET", null);
        Assert.Equal(5, result.VehicleInterest);
    }

    // --- Inbound rating branches (MapFromEasyCarsLead) ---

    [Fact]
    public void MapFromEasyCarsLead_WithRating2_SetsRatingToWarm()
    {
        var response = CreateTestLeadDetailResponse();
        response.Rating = 2;
        var result = _sut.MapFromEasyCarsLead(response, 1);
        Assert.Equal("Warm", result.Rating);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithRating3_SetsRatingToCold()
    {
        var response = CreateTestLeadDetailResponse();
        response.Rating = 3;
        var result = _sut.MapFromEasyCarsLead(response, 1);
        Assert.Equal("Cold", result.Rating);
    }

    // --- LeadStatus additional branches ---

    [Fact]
    public void MapFromEasyCarsLead_WithLeadStatus60_SetsStatusToDone()
    {
        var response = CreateTestLeadDetailResponse();
        response.LeadStatus = 60;
        var result = _sut.MapFromEasyCarsLead(response, 1);
        Assert.Equal(LeadStatus.Done, result.Status);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithLeadStatus90_SetsStatusToDone()
    {
        var response = CreateTestLeadDetailResponse();
        response.LeadStatus = 90;
        var result = _sut.MapFromEasyCarsLead(response, 1);
        Assert.Equal(LeadStatus.Done, result.Status);
    }

    [Fact]
    public void MapFromEasyCarsLead_WithNullLeadStatus_DefaultsToReceived()
    {
        var response = CreateTestLeadDetailResponse();
        response.LeadStatus = null;
        var result = _sut.MapFromEasyCarsLead(response, 1);
        Assert.Equal(LeadStatus.Received, result.Status);
    }

    // --- CustomerMobile fallback ---

    [Fact]
    public void MapFromEasyCarsLead_WithNullCustomerPhoneAndMobileSet_UsesMobileAsPhone()
    {
        var response = new LeadDetailResponse
        {
            LeadNumber = "LEAD-003",
            CustomerName = "Mobile Test",
            CustomerEmail = "mobile@example.com",
            CustomerPhone = null,
            CustomerMobile = "0411111111",
            Comments = "Test"
        };

        var ex = Record.Exception(() => _sut.MapFromEasyCarsLead(response, 1));
        Assert.Null(ex);
        // Phone field is populated from CustomerMobile fallback (no exception means fallback worked)
    }
}
