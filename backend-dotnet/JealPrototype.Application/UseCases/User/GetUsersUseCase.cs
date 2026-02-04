using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.User;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.User;

public class GetUsersUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersUseCase(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<UserResponseDto>>> ExecuteAsync(
        int requestorId,
        int? dealershipId,
        CancellationToken cancellationToken = default)
    {
        // Get the requestor to check their user type
        var requestor = await _userRepository.GetByIdAsync(requestorId, cancellationToken);
        if (requestor == null)
        {
            return ApiResponse<List<UserResponseDto>>.ErrorResponse("User not found");
        }

        List<Domain.Entities.User> users;

        // Admin can see all users across all dealerships
        if (requestor.UserType == UserType.Admin)
        {
            users = (await _userRepository.GetAllAsync(cancellationToken)).ToList();
        }
        // Dealership owners and staff can only see users from their dealership
        else if (dealershipId.HasValue)
        {
            users = (await _userRepository.GetByDealershipIdAsync(dealershipId.Value, cancellationToken)).ToList();
        }
        else
        {
            return ApiResponse<List<UserResponseDto>>.ErrorResponse("Dealership ID required");
        }

        var response = _mapper.Map<List<UserResponseDto>>(users);
        return ApiResponse<List<UserResponseDto>>.SuccessResponse(response);
    }
}
