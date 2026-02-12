using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.User;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.User;

public class GetDealershipUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public GetDealershipUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<List<UserResponseDto>>> ExecuteAsync(
        int dealershipId,
        int? requestorId = null,
        CancellationToken cancellationToken = default)
    {
        // If requestor ID provided, validate tenant isolation
        if (requestorId.HasValue)
        {
            var requestor = await _userRepository.GetByIdAsync(requestorId.Value, cancellationToken);
            if (requestor == null)
            {
                return ApiResponse<List<UserResponseDto>>.ErrorResponse("Requestor not found");
            }

            // Admin can access any dealership's users
            if (requestor.UserType != UserType.Admin)
            {
                // Non-admin users can only access their own dealership's users
                if (!requestor.DealershipId.HasValue || requestor.DealershipId.Value != dealershipId)
                {
                    return ApiResponse<List<UserResponseDto>>.ErrorResponse(
                        "Access denied: You can only view users from your own dealership");
                }
            }
        }

        var users = await _userRepository.GetByDealershipIdAsync(dealershipId, cancellationToken);

        var userDtos = users.Select(user => new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email.Value,
            FullName = user.FullName,
            UserType = user.UserType.ToString().ToLower().Replace("dealership", "dealership_"),
            DealershipId = user.DealershipId,
            Permissions = user.Permissions.Select(p => p.ToString().ToLower()).ToList(),
            CreatedBy = user.CreatedBy,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        }).ToList();

        return ApiResponse<List<UserResponseDto>>.SuccessResponse(userDtos);
    }
}
