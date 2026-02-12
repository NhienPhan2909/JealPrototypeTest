using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.User;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Application.UseCases.User;

public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public CreateUserUseCase(IUserRepository userRepository, IAuthService authService, IMapper mapper)
    {
        _userRepository = userRepository;
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserResponseDto>> ExecuteAsync(
        CreateUserDto request,
        int creatorId,
        CancellationToken cancellationToken = default)
    {
        // Fetch creator to validate authorization
        var creator = await _userRepository.GetByIdAsync(creatorId, cancellationToken);
        if (creator == null)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse("Creator not found");
        }

        // Determine target user type (with privilege restrictions)
        var targetUserType = request.UserType.ToLower().Replace("_", "") switch
        {
            "admin" => UserType.Admin,
            "dealershipowner" => UserType.DealershipOwner,
            "dealershipstaff" => UserType.DealershipStaff,
            _ => UserType.DealershipStaff
        };

        // PRIVILEGE ESCALATION PREVENTION: Only Admin can create Admin users
        if (targetUserType == UserType.Admin && creator.UserType != UserType.Admin)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse(
                "Only system administrators can create admin users");
        }

        // PRIVILEGE ESCALATION PREVENTION: Only Admin can create DealershipOwner users
        if (targetUserType == UserType.DealershipOwner && creator.UserType != UserType.Admin)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse(
                "Only system administrators can create dealership owners");
        }

        // If creator is not Admin, new user must be in same dealership
        if (creator.UserType != UserType.Admin)
        {
            if (!creator.DealershipId.HasValue)
            {
                return ApiResponse<UserResponseDto>.ErrorResponse("Creator has no dealership association");
            }

            // Force new user to be in creator's dealership
            if (request.DealershipId.HasValue && request.DealershipId.Value != creator.DealershipId.Value)
            {
                return ApiResponse<UserResponseDto>.ErrorResponse(
                    "You can only create users for your own dealership");
            }

            // Ensure dealership ID is set
            request.DealershipId = creator.DealershipId.Value;
        }

        // Check if username already exists
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse("Username already exists");
        }

        var userType = targetUserType;

        var permissions = request.Permissions?.Select(p => p.ToUpper() switch
        {
            "VEHICLES" => Permission.Vehicles,
            "LEADS" => Permission.Leads,
            "SALESREQUESTS" => Permission.SalesRequests,
            "SALES_REQUESTS" => Permission.SalesRequests,
            "BLOGS" => Permission.Blogs,
            "SETTINGS" => Permission.Settings,
            _ => Permission.Vehicles
        }).ToList() ?? new List<Permission>();

        var passwordHash = _authService.HashPassword(request.Password);

        var user = Domain.Entities.User.Create(
            request.Username,
            passwordHash,
            request.Email,
            request.FullName,
            userType,
            request.DealershipId,
            permissions,
            null);

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);
        var response = _mapper.Map<UserResponseDto>(createdUser);

        return ApiResponse<UserResponseDto>.SuccessResponse(response, "User created successfully");
    }
}
