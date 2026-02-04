using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Enums;
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
        int deleterId,
        CancellationToken cancellationToken = default)
    {
        // Get the user to be deleted
        var userToDelete = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (userToDelete == null)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        // Get the deleter (person trying to delete)
        var deleter = await _userRepository.GetByIdAsync(deleterId, cancellationToken);
        if (deleter == null)
        {
            return ApiResponse<bool>.ErrorResponse("Deleter not found");
        }

        // Cannot delete yourself
        if (userId == deleterId)
        {
            return ApiResponse<bool>.ErrorResponse("You cannot delete yourself");
        }

        // Authorization checks based on user hierarchy
        // Admin can delete anyone
        if (deleter.UserType == UserType.Admin)
        {
            await _userRepository.DeleteAsync(userToDelete, cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
        }

        // Dealership Owner can delete staff from their dealership
        if (deleter.UserType == UserType.DealershipOwner)
        {
            if (userToDelete.UserType != UserType.DealershipStaff)
            {
                return ApiResponse<bool>.ErrorResponse("Dealership owners can only delete staff users");
            }

            if (userToDelete.DealershipId != deleter.DealershipId)
            {
                return ApiResponse<bool>.ErrorResponse("You can only delete users from your dealership");
            }

            await _userRepository.DeleteAsync(userToDelete, cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
        }

        // Dealership Staff cannot delete users
        return ApiResponse<bool>.ErrorResponse("You do not have permission to delete users");
    }
}
