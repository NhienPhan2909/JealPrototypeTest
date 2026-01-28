using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.User;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Application.UseCases.User;

public class UpdateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public UpdateUserUseCase(IUserRepository userRepository, IAuthService authService, IMapper mapper)
    {
        _userRepository = userRepository;
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserResponseDto>> ExecuteAsync(
        int userId,
        UpdateUserDto request,
        int requestorDealershipId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.DealershipId != requestorDealershipId)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse("User not found");
        }

        var email = !string.IsNullOrWhiteSpace(request.Email)
            ? Email.Create(request.Email)
            : user.Email;

        var userType = !string.IsNullOrWhiteSpace(request.UserType)
            ? request.UserType.ToLower() switch
            {
                "admin" => UserType.Admin,
                "dealershipowner" => UserType.DealershipOwner,
                "dealershipstaff" => UserType.DealershipStaff,
                _ => user.UserType
            }
            : user.UserType;

        var permissions = request.Permissions?.Select(p => p.ToUpper() switch
        {
            "VEHICLES" => Permission.Vehicles,
            "LEADS" => Permission.Leads,
            "SALESREQUESTS" => Permission.SalesRequests,
            "BLOGS" => Permission.Blogs,
            "SETTINGS" => Permission.Settings,
            _ => Permission.Vehicles
        }).ToList();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.UpdateProfile(request.Email, user.FullName);
        }

        if (request.Permissions != null && user.UserType == UserType.DealershipStaff)
        {
            user.UpdatePermissions(permissions!);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var newPasswordHash = _authService.HashPassword(request.Password);
            user.UpdatePassword(newPasswordHash);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        var response = _mapper.Map<UserResponseDto>(user);
        return ApiResponse<UserResponseDto>.SuccessResponse(response, "User updated successfully");
    }
}
