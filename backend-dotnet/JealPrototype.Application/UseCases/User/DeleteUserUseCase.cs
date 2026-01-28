using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.User;

public class DeleteUserUseCase
{
    private readonly IUserRepository _userRepository;

    public DeleteUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<bool>> ExecuteAsync(
        int userId,
        int requestorDealershipId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.DealershipId != requestorDealershipId)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
    }
}
