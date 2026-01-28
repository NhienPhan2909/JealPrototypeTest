using AutoMapper;
using JealPrototype.Application.DTOs.Auth;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public LoginUseCase(
        IUserRepository userRepository,
        IAuthService authService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _authService = authService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<LoginResponseDto>> ExecuteAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid username or password");
        }

        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid username or password");
        }

        var token = _authService.GenerateJwtToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        var response = new LoginResponseDto
        {
            Token = token,
            User = userDto
        };

        return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful");
    }
}
