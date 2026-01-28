using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Vehicle;

public class GetVehicleByIdUseCase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public GetVehicleByIdUseCase(IVehicleRepository vehicleRepository, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<VehicleResponseDto>> ExecuteAsync(
        int vehicleId,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAndDealershipAsync(vehicleId, dealershipId, cancellationToken);

        if (vehicle == null)
        {
            return ApiResponse<VehicleResponseDto>.ErrorResponse("Vehicle not found");
        }

        var response = _mapper.Map<VehicleResponseDto>(vehicle);
        return ApiResponse<VehicleResponseDto>.SuccessResponse(response);
    }
}
