using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Users;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(EventFlowDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResponse<UserResponse>> GetUsersAsync(UserQueryParameters query)
    {
        var queryable = _dbContext.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(u =>
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        if (query.ApplicationId.HasValue)
        {
            queryable = queryable.Where(u => u.ApplicationId == query.ApplicationId.Value);
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(u => u.Status == query.Status.Value);
        }

        var totalCount = await queryable.CountAsync();

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var users = await queryable
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<UserResponse>
        {
            Items = users.Select(MapToUserResponse),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid id)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException($"User with id '{id}' was not found.");
        }

        return MapToUserResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == email && !u.IsDeleted);

        if (emailExists)
        {
            throw new BadRequestException("User with this email already exists.");
        }

        var appExists = await _dbContext.Applications
            .AnyAsync(a => a.Id == request.ApplicationId && !a.IsDeleted);

        if (!appExists)
        {
            throw new NotFoundException($"Application with id '{request.ApplicationId}' was not found.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.ApplicationAdmin,
            Status = UserStatus.Active,
            ApplicationId = request.ApplicationId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return MapToUserResponse(user);
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException($"User with id '{id}' was not found.");
        }

        var email = request.Email.Trim().ToLower();
        if (user.Email.ToLower() != email)
        {
            var emailExists = await _dbContext.Users
                .AnyAsync(u => u.Email.ToLower() == email && !u.IsDeleted);

            if (emailExists)
            {
                throw new BadRequestException("User with this email already exists.");
            }
            user.Email = email;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        if (user.Role != UserRole.RootAdmin)
        {
            if (request.ApplicationId.HasValue)
            {
                var appExists = await _dbContext.Applications
                    .AnyAsync(a => a.Id == request.ApplicationId.Value && !a.IsDeleted);

                if (!appExists)
                {
                    throw new NotFoundException($"Application with id '{request.ApplicationId.Value}' was not found.");
                }
                user.ApplicationId = request.ApplicationId.Value;
            }
        }
        else
        {
            user.ApplicationId = null;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return MapToUserResponse(user);
    }

    public async Task UpdateUserStatusAsync(Guid id, UpdateUserStatusRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException($"User with id '{id}' was not found.");
        }

        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException($"User with id '{id}' was not found.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException($"User with id '{id}' was not found.");
        }

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
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
