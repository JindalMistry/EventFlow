using EventFlow.Application.DTOs.Auth;
using EventFlow.Application.DTOs.Users;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Configuration;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventFlow.Infrastructure.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        EventFlowDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedException($"User account is {user.Status.ToString().ToLower()}.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresIn = _jwtSettings.ExpiryMinutes * 60,
            User = MapToUserResponse(user)
        };
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (!_passwordHasher.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new BadRequestException("Invalid old password.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserResponse> GetMeAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return MapToUserResponse(user);
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            ApplicationId = user.ApplicationId,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
