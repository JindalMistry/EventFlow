using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Users;

namespace EventFlow.Application.Interfaces;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> GetUsersAsync(UserQueryParameters query);
    Task<UserResponse> GetUserByIdAsync(Guid id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task UpdateUserStatusAsync(Guid id, UpdateUserStatusRequest request);
    Task ResetPasswordAsync(Guid id, ResetPasswordRequest request);
    Task DeleteUserAsync(Guid id);
}
