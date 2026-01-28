using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Vehicle;

public class GetVehiclesUseCase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public GetVehiclesUseCase(IVehicleRepository vehicleRepository, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<VehicleResponseDto>>> ExecuteAsync(
        int dealershipId,
        VehicleFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        VehicleStatus? status = null;
        if (!string.IsNullOrWhiteSpace(filter?.Status))
        {
            status = filter.Status.ToLower() switch
            {
                "draft" => VehicleStatus.Draft,
                "active" => VehicleStatus.Active,
                "pending" => VehicleStatus.Pending,
                "sold" => VehicleStatus.Sold,
                _ => null
            };
        }

        var vehicles = await _vehicleRepository.GetByDealershipIdAsync(
            dealershipId,
            status,
            filter?.Brand,
            filter?.MinYear,
            filter?.MaxYear,
            filter?.MinPrice,
            filter?.MaxPrice,
            cancellationToken);

        var response = _mapper.Map<List<VehicleResponseDto>>(vehicles);
        return ApiResponse<List<VehicleResponseDto>>.SuccessResponse(response);
    }
}
