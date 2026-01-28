using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.User;
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
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByDealershipIdAsync(dealershipId, cancellationToken);
        var response = _mapper.Map<List<UserResponseDto>>(users);

        return ApiResponse<List<UserResponseDto>>.SuccessResponse(response);
    }
}
