using System.Text.Json;
using EventFlow.Application.Common;
using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class ProviderConfigurationService : IProviderConfigurationService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IProviderTestService _providerTestService;
    private readonly IProviderConfigurationValidator _validator;

    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "authToken", "secret", "apiSecret", "token", "apiKey", "privateKey", "credential", "credentials"
    };

    public ProviderConfigurationService(
        EventFlowDbContext dbContext,
        ICurrentUserService currentUser,
        IProviderTestService providerTestService,
        IProviderConfigurationValidator validator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _providerTestService = providerTestService;
        _validator = validator;
    }

    public async Task<ProviderConfigurationResponse> CreateProviderConfigurationAsync(CreateProviderConfigurationRequest request)
    {
        var targetApplicationId = request.ApplicationId;

        if (_currentUser.IsApplicationAdmin)
        {
            if (!_currentUser.ApplicationId.HasValue)
            {
                throw new ForbiddenException("ApplicationAdmin user does not have an assigned ApplicationId.");
            }

            if (targetApplicationId != Guid.Empty && targetApplicationId != _currentUser.ApplicationId.Value)
            {
                throw new ForbiddenException("ApplicationAdmin can only create provider configurations for their own application.");
            }

            targetApplicationId = _currentUser.ApplicationId.Value;
        }

        if (targetApplicationId == Guid.Empty)
        {
            throw new BadRequestException("ApplicationId is required.");
        }

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == targetApplicationId && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{targetApplicationId}' was not found.");
        }

        if (application.Status != ApplicationStatus.Active)
        {
            throw new BadRequestException("Cannot create provider configuration for an inactive application.");
        }

        if (!Enum.IsDefined(typeof(ProviderType), request.ProviderType))
        {
            throw new BadRequestException("Invalid provider type.");
        }

        var trimmedDisplayName = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedDisplayName))
        {
            throw new BadRequestException("DisplayName is required.");
        }

        var validatedConfiguration = await _validator.ValidateAndDeserializeAsync(request.ProviderType, request.Configuration);

        var duplicateExists = await _dbContext.ProviderConfigurations
            .AnyAsync(p => p.ApplicationId == targetApplicationId &&
                           p.ProviderType == request.ProviderType &&
                           p.DisplayName.ToLower() == trimmedDisplayName.ToLower() &&
                           !p.IsDeleted);

        if (duplicateExists)
        {
            throw new BadRequestException("A provider configuration with the same Application, ProviderType, and DisplayName already exists.");
        }

        var configurationJson = JsonSerializer.Serialize(validatedConfiguration, ProviderConfigurationValidator.SerializerOptions);
        var now = DateTime.UtcNow;

        var entity = new ProviderConfiguration
        {
            Id = Guid.NewGuid(),
            ApplicationId = targetApplicationId,
            ProviderType = request.ProviderType,
            DisplayName = trimmedDisplayName,
            IsEnabled = request.IsEnabled,
            Configuration = configurationJson,
            CreatedAt = now,
            CreatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null
        };

        _dbContext.ProviderConfigurations.Add(entity);
        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<PagedResponse<ProviderConfigurationResponse>> GetProviderConfigurationsAsync(ProviderConfigurationQueryParameters query)
    {
        var queryable = _dbContext.ProviderConfigurations
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (_currentUser.IsApplicationAdmin)
        {
            if (!_currentUser.ApplicationId.HasValue)
            {
                throw new ForbiddenException("ApplicationAdmin user does not have an assigned ApplicationId.");
            }

            if (query.ApplicationId.HasValue && query.ApplicationId.Value != _currentUser.ApplicationId.Value)
            {
                throw new ForbiddenException("ApplicationAdmin can only view provider configurations for their own application.");
            }

            queryable = queryable.Where(p => p.ApplicationId == _currentUser.ApplicationId.Value);
        }
        else if (_currentUser.IsRootAdmin)
        {
            if (query.ApplicationId.HasValue)
            {
                queryable = queryable.Where(p => p.ApplicationId == query.ApplicationId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(p => p.DisplayName.ToLower().Contains(search));
        }

        if (query.ProviderType.HasValue)
        {
            queryable = queryable.Where(p => p.ProviderType == query.ProviderType.Value);
        }

        if (query.IsEnabled.HasValue)
        {
            queryable = queryable.Where(p => p.IsEnabled == query.IsEnabled.Value);
        }

        var totalCount = await queryable.CountAsync();
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var items = await queryable
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ProviderConfigurationResponse>
        {
            Items = items.Select(MapToResponse),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProviderConfigurationResponse> GetProviderConfigurationByIdAsync(Guid id)
    {
        var entity = await _dbContext.ProviderConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Provider configuration with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        return MapToResponse(entity);
    }

    public async Task<ProviderConfigurationResponse> UpdateProviderConfigurationAsync(Guid id, UpdateProviderConfigurationRequest request)
    {
        var entity = await _dbContext.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Provider configuration with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        if (!Enum.IsDefined(typeof(ProviderType), request.ProviderType))
        {
            throw new BadRequestException("Invalid provider type.");
        }

        var trimmedDisplayName = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedDisplayName))
        {
            throw new BadRequestException("DisplayName is required.");
        }

        var duplicateExists = await _dbContext.ProviderConfigurations
            .AnyAsync(p => p.Id != id &&
                           p.ApplicationId == entity.ApplicationId &&
                           p.ProviderType == request.ProviderType &&
                           p.DisplayName.ToLower() == trimmedDisplayName.ToLower() &&
                           !p.IsDeleted);

        if (duplicateExists)
        {
            throw new BadRequestException("A provider configuration with the same Application, ProviderType, and DisplayName already exists.");
        }

        var validatedConfiguration = await _validator.ValidateAndDeserializeAsync(request.ProviderType, request.Configuration);

        entity.DisplayName = trimmedDisplayName;
        entity.ProviderType = request.ProviderType;
        entity.Configuration = JsonSerializer.Serialize(validatedConfiguration, ProviderConfigurationValidator.SerializerOptions);
        entity.IsEnabled = request.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<ProviderConfigurationResponse> EnableProviderConfigurationAsync(Guid id)
    {
        var entity = await _dbContext.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Provider configuration with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == entity.ApplicationId && !a.IsDeleted);

        if (application == null || application.Status != ApplicationStatus.Active)
        {
            throw new BadRequestException("Cannot enable provider configuration because the associated application is inactive or deleted.");
        }

        _validator.Deserialize(entity.ProviderType, entity.Configuration);

        entity.IsEnabled = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<ProviderConfigurationResponse> DisableProviderConfigurationAsync(Guid id)
    {
        var entity = await _dbContext.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Provider configuration with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        entity.IsEnabled = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<TestProviderConnectionResponse> TestProviderConnectionAsync(Guid id)
    {
        var entity = await _dbContext.ProviderConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Provider configuration with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        return await _providerTestService.TestConnectionAsync(entity.ProviderType, entity.Configuration);
    }

    private void ValidateTenantAccess(Guid applicationId)
    {
        if (_currentUser.IsApplicationAdmin)
        {
            if (!_currentUser.ApplicationId.HasValue || _currentUser.ApplicationId.Value != applicationId)
            {
                throw new ForbiddenException("ApplicationAdmin can only access provider configurations belonging to their own application.");
            }
        }
    }


    private static ProviderConfigurationResponse MapToResponse(ProviderConfiguration entity)
    {
        return new ProviderConfigurationResponse
        {
            Id = entity.Id,
            ApplicationId = entity.ApplicationId,
            ProviderType = entity.ProviderType.ToString(),
            DisplayName = entity.DisplayName,
            IsEnabled = entity.IsEnabled,
            Configuration = SanitizeConfiguration(entity.Configuration),
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId,
            UpdatedAt = entity.UpdatedAt,
            UpdatedByUserId = entity.UpdatedByUserId
        };
    }

    private static JsonElement SanitizeConfiguration(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return JsonSerializer.Deserialize<JsonElement>("{}");
        }

        try
        {
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return root.Clone();
            }

            var dict = new Dictionary<string, object?>();
            foreach (var property in root.EnumerateObject())
            {
                dict[property.Name] = property.Value.Clone();
            }

            var sanitizedJson = JsonSerializer.Serialize(dict);
            return JsonSerializer.Deserialize<JsonElement>(sanitizedJson);
        }
        catch
        {
            return JsonSerializer.Deserialize<JsonElement>("{}");
        }
    }

    private static bool IsSensitiveKey(string keyName)
    {
        return SensitiveKeys.Any(s => keyName.Contains(s, StringComparison.OrdinalIgnoreCase));
    }
}
