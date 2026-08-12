using System.Text.RegularExpressions;
using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Applications;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = EventFlow.Domain.Entities.Application;

namespace EventFlow.Infrastructure.Services;

public class ApplicationService : IApplicationService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationService(EventFlowDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateApplicationResponse> CreateApplicationAsync(CreateApplicationRequest request)
    {
        var trimmedName = request.Name.Trim();
        var nameExists = await _dbContext.Applications
            .AnyAsync(a => a.Name.ToLower() == trimmedName.ToLower() && !a.IsDeleted);

        if (nameExists)
        {
            throw new BadRequestException("Application name must be unique.");
        }

        var code = GenerateApplicationCode(trimmedName);
        var codeExists = await _dbContext.Applications
            .AnyAsync(a => a.Code.ToUpper() == code.ToUpper() && !a.IsDeleted);

        if (codeExists)
        {
            code = GenerateApplicationCode(trimmedName);
        }

        var plainApiKey = $"ef_{Guid.NewGuid():N}{Guid.NewGuid():N}";
        var apiKeyHash = _passwordHasher.Hash(plainApiKey);

        var now = DateTime.UtcNow;

        var application = new ApplicationEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = trimmedName,
            Description = request.Description?.Trim(),
            SupportEmail = request.SupportEmail?.Trim(),
            ApiKeyHash = apiKeyHash,
            Status = ApplicationStatus.Active,
            ApiKeyCreatedAt = now,
            CreatedAt = now
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        return new CreateApplicationResponse
        {
            Id = application.Id,
            Code = application.Code,
            Name = application.Name,
            Description = application.Description,
            SupportEmail = application.SupportEmail,
            Status = application.Status.ToString(),
            HasApiKey = true,
            ApiKeyCreatedAt = application.ApiKeyCreatedAt,
            ApiKeyExpiresAt = application.ApiKeyExpiresAt,
            CreatedAt = application.CreatedAt,
            ApiKey = plainApiKey
        };
    }

    public async Task<ApplicationResponse> UpdateApplicationAsync(Guid id, UpdateApplicationRequest request)
    {
        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{id}' was not found.");
        }

        var trimmedName = request.Name.Trim();
        if (application.Name.ToLower() != trimmedName.ToLower())
        {
            var nameExists = await _dbContext.Applications
                .AnyAsync(a => a.Name.ToLower() == trimmedName.ToLower() && !a.IsDeleted);

            if (nameExists)
            {
                throw new BadRequestException("Application name must be unique.");
            }
            application.Name = trimmedName;
        }

        application.Description = request.Description?.Trim();
        application.SupportEmail = request.SupportEmail?.Trim();
        application.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToApplicationResponse(application);
    }

    public async Task<PagedResponse<ApplicationResponse>> GetApplicationsAsync(ApplicationQueryParameters query)
    {
        var queryable = _dbContext.Applications
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(a =>
                a.Name.ToLower().Contains(search) ||
                a.Code.ToLower().Contains(search));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(a => a.Status == query.Status.Value);
        }

        var totalCount = await queryable.CountAsync();

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var applications = await queryable
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ApplicationResponse>
        {
            Items = applications.Select(MapToApplicationResponse),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ApplicationDetailResponse> GetApplicationByIdAsync(Guid id)
    {
        var application = await _dbContext.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{id}' was not found.");
        }

        var usersCount = await _dbContext.Users
            .CountAsync(u => u.ApplicationId == id && !u.IsDeleted);

        var providersCount = await _dbContext.ProviderConfigurations
            .CountAsync(p => p.ApplicationId == id && !p.IsDeleted);

        var eventsCount = await _dbContext.EventDefinitions
            .CountAsync(e => e.ApplicationId == id && !e.IsDeleted);

        var rulesCount = await _dbContext.NotificationRules
            .CountAsync(r => r.ApplicationId == id && !r.IsDeleted);

        var templatesCount = await _dbContext.NotificationTemplates
            .CountAsync(t => t.ApplicationId == id && !t.IsDeleted);

        return new ApplicationDetailResponse
        {
            Id = application.Id,
            Code = application.Code,
            Name = application.Name,
            Description = application.Description,
            SupportEmail = application.SupportEmail,
            Status = application.Status.ToString(),
            HasApiKey = !string.IsNullOrEmpty(application.ApiKeyHash),
            ApiKeyCreatedAt = application.ApiKeyCreatedAt,
            ApiKeyExpiresAt = application.ApiKeyExpiresAt,
            CreatedAt = application.CreatedAt,
            TotalApplicationAdminUsers = usersCount,
            TotalProviderConfigurations = providersCount,
            TotalEventDefinitions = eventsCount,
            TotalNotificationRules = rulesCount,
            TotalNotificationTemplates = templatesCount
        };
    }

    public async Task DeleteApplicationAsync(Guid id)
    {
        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{id}' was not found.");
        }

        application.IsDeleted = true;
        application.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task ActivateApplicationAsync(Guid id)
    {
        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{id}' was not found.");
        }

        application.Status = ApplicationStatus.Active;
        application.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeactivateApplicationAsync(Guid id)
    {
        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{id}' was not found.");
        }

        application.Status = ApplicationStatus.Inactive;
        application.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<RotateApiKeyResponse> RotateApiKeyAsync(Guid id)
    {
        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{id}' was not found.");
        }

        var newPlainApiKey = $"ef_{Guid.NewGuid():N}{Guid.NewGuid():N}";
        application.ApiKeyHash = _passwordHasher.Hash(newPlainApiKey);
        application.ApiKeyCreatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new RotateApiKeyResponse
        {
            ApplicationId = application.Id,
            ApiKey = newPlainApiKey,
            ApiKeyCreatedAt = application.ApiKeyCreatedAt
        };
    }

    private static string GenerateApplicationCode(string name)
    {
        var uppercase = name.Trim().ToUpperInvariant();
        var spacesReplaced = Regex.Replace(uppercase, @"\s+", "_");
        var cleaned = Regex.Replace(spacesReplaced, @"[^A-Z0-9_]", "");
        var guidSuffix = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return $"{cleaned}_{guidSuffix}";
    }

    private static ApplicationResponse MapToApplicationResponse(ApplicationEntity application)
    {
        return new ApplicationResponse
        {
            Id = application.Id,
            Code = application.Code,
            Name = application.Name,
            Description = application.Description,
            SupportEmail = application.SupportEmail,
            Status = application.Status.ToString(),
            HasApiKey = !string.IsNullOrEmpty(application.ApiKeyHash),
            ApiKeyCreatedAt = application.ApiKeyCreatedAt,
            ApiKeyExpiresAt = application.ApiKeyExpiresAt,
            CreatedAt = application.CreatedAt
        };
    }
}
