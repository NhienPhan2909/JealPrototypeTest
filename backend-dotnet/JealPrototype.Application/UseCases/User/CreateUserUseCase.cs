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
        int creatorDealershipId,
        CancellationToken cancellationToken = default)
    {
        // Check if username already exists
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse("Username already exists");
        }

        var userType = request.UserType.ToLower() switch
        {
            "admin" => UserType.Admin,
            "dealershipowner" => UserType.DealershipOwner,
            "dealershipstaff" => UserType.DealershipStaff,
            _ => UserType.DealershipStaff
        };

        var permissions = request.Permissions?.Select(p => p.ToUpper() switch
        {
            "VEHICLES" => Permission.Vehicles,
            "LEADS" => Permission.Leads,
            "SALESREQUESTS" => Permission.SalesRequests,
            "BLOGS" => Permission.Blogs,
            "SETTINGS" => Permission.Settings,
            _ => Permission.Vehicles
        }).ToList() ?? new List<Permission>();

        var passwordHash = _authService.HashPassword(request.Password);

        var user = Domain.Entities.User.Create(
            request.Username,
            passwordHash,
            request.Email,
            request.Username, // Using username as fullName since CreateUserDto doesn't have FullName
            userType,
            request.DealershipId ?? creatorDealershipId,
            permissions,
            null);

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);
        var response = _mapper.Map<UserResponseDto>(createdUser);

        return ApiResponse<UserResponseDto>.SuccessResponse(response, "User created successfully");
    }
}
