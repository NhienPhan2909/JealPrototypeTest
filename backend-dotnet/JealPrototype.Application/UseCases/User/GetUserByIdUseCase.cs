using JealPrototype.Application.DTOs.User;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.User;

public class GetUserByIdUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto?> ExecuteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserResponseDto
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
        };
    }
}
