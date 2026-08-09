using EventFlow.Application.DTOs.Auth;
using EventFlow.Application.DTOs.Users;

namespace EventFlow.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserResponse> GetMeAsync(Guid userId);
}
