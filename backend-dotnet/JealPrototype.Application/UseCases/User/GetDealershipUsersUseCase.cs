using JealPrototype.Application.DTOs.User;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.User;

public class GetDealershipUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public GetDealershipUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> ExecuteAsync(int dealershipId)
    {
        var users = await _userRepository.GetByDealershipIdAsync(dealershipId);

        return users.Select(user => new UserResponseDto
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
        });
    }
}
