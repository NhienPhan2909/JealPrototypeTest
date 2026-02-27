using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.Interfaces;

public interface IEasyCarsLeadMapper
{
    CreateLeadRequest MapToCreateLeadRequest(Lead lead, string accountNumber, string accountSecret, Vehicle? vehicle);
    UpdateLeadRequest MapToUpdateLeadRequest(Lead lead, string leadNumber, string accountNumber, string accountSecret, Vehicle? vehicle);
    Lead MapFromEasyCarsLead(LeadDetailResponse response, int dealershipId);
    void UpdateLeadFromResponse(Lead lead, LeadDetailResponse response);
    bool IsExistingLead(Lead lead, LeadDetailResponse response);
    /// <summary>Maps local LeadStatus enum to EasyCars integer status.</summary>
    int MapLeadStatusToEasyCars(LeadStatus status);
    /// <summary>Maps EasyCars integer status to local LeadStatus enum.</summary>
    LeadStatus MapLeadStatusFromInt(int easyCarsStatus);
    /// <summary>Builds a status-only UpdateLeadRequest.</summary>
    UpdateLeadRequest MapToStatusOnlyUpdateRequest(Lead lead, string accountNumber, string accountSecret);
}
