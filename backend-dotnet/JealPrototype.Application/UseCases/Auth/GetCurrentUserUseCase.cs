using AutoMapper;
using JealPrototype.Application.DTOs.Auth;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Auth;

public class GetCurrentUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetCurrentUserUseCase(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserDto>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResponse("User not found");
        }

        var userDto = _mapper.Map<UserDto>(user);
        return ApiResponse<UserDto>.SuccessResponse(userDto);
    }
}
