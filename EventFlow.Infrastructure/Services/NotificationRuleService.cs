using EventFlow.Application.Common;
using EventFlow.Application.DTOs.NotificationRules;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class NotificationRuleService : INotificationRuleService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public NotificationRuleService(EventFlowDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<NotificationRuleDetailResponse> CreateNotificationRuleAsync(CreateNotificationRuleRequest request)
    {
        var tenantId = GetApplicationAdminTenantId();

        ValidateRuleParameters(request.RetryCount, request.DelayInSeconds, request.Priority, request.Channel);

        var (eventDef, template, provider) = await ValidateAndGetDependenciesAsync(
            tenantId,
            request.EventDefinitionId,
            request.NotificationTemplateId,
            request.ProviderConfigurationId,
            request.Channel);

        var now = DateTime.UtcNow;

        var entity = new NotificationRule
        {
            Id = Guid.NewGuid(),
            ApplicationId = tenantId,
            EventDefinitionId = request.EventDefinitionId,
            NotificationTemplateId = request.NotificationTemplateId,
            ProviderConfigurationId = request.ProviderConfigurationId,
            Channel = request.Channel,
            RetryCount = request.RetryCount,
            DelayInSeconds = request.DelayInSeconds,
            Priority = request.Priority,
            IsEnabled = request.IsEnabled,
            CreatedAt = now,
            CreatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null,
            EventDefinition = eventDef,
            NotificationTemplate = template,
            ProviderConfiguration = provider
        };

        _dbContext.NotificationRules.Add(entity);
        await _dbContext.SaveChangesAsync();

        return MapToDetailResponse(entity);
    }

    public async Task<PagedResponse<NotificationRuleResponse>> GetNotificationRulesAsync(NotificationRuleQueryParameters query)
    {
        var tenantId = GetApplicationAdminTenantId();

        var queryable = _dbContext.NotificationRules
            .AsNoTracking()
            .Include(r => r.EventDefinition)
            .Include(r => r.NotificationTemplate)
            .Include(r => r.ProviderConfiguration)
            .Where(r => r.ApplicationId == tenantId && !r.IsDeleted);

        if (query.EventDefinitionId.HasValue)
        {
            queryable = queryable.Where(r => r.EventDefinitionId == query.EventDefinitionId.Value);
        }

        if (query.Channel.HasValue)
        {
            queryable = queryable.Where(r => r.Channel == query.Channel.Value);
        }

        if (query.IsEnabled.HasValue)
        {
            queryable = queryable.Where(r => r.IsEnabled == query.IsEnabled.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(r =>
                r.EventDefinition.Code.ToLower().Contains(search) ||
                r.EventDefinition.Name.ToLower().Contains(search) ||
                r.ProviderConfiguration.DisplayName.ToLower().Contains(search));
        }

        var totalCount = await queryable.CountAsync();
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var items = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<NotificationRuleResponse>
        {
            Items = items.Select(MapToSummaryResponse),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<NotificationRuleDetailResponse> GetNotificationRuleByIdAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationRules
            .AsNoTracking()
            .Include(r => r.EventDefinition)
            .Include(r => r.NotificationTemplate)
            .Include(r => r.ProviderConfiguration)
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationId == tenantId && !r.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification rule with id '{id}' was not found.");
        }

        return MapToDetailResponse(entity);
    }

    public async Task<List<NotificationRuleDetailResponse>> GetNotificationRulesByEventIdAsync(Guid id, CancellationToken token)
    {
        List<NotificationRuleDetailResponse> res = new List<NotificationRuleDetailResponse>();

        var data = await _dbContext.NotificationRules
            .AsNoTracking()
            .Include(r => r.EventDefinition)
            .Include(r => r.NotificationTemplate)
            .Include(r => r.ProviderConfiguration)
            .Where(r => r.EventDefinitionId == id)
            .ToListAsync();


        foreach (var d in data)
        {
            res.Add(MapToDetailResponse(d));
        }

        return res;
    }

    public async Task<NotificationRuleDetailResponse> UpdateNotificationRuleAsync(Guid id, UpdateNotificationRuleRequest request)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationRules
            .Include(r => r.EventDefinition)
            .Include(r => r.NotificationTemplate)
            .Include(r => r.ProviderConfiguration)
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationId == tenantId && !r.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification rule with id '{id}' was not found.");
        }

        ValidateRuleParameters(request.RetryCount, request.DelayInSeconds, request.Priority, request.Channel);

        var (_, template, provider) = await ValidateAndGetDependenciesAsync(
            tenantId,
            entity.EventDefinitionId,
            request.NotificationTemplateId,
            request.ProviderConfigurationId,
            request.Channel);

        entity.NotificationTemplateId = request.NotificationTemplateId;
        entity.ProviderConfigurationId = request.ProviderConfigurationId;
        entity.Channel = request.Channel;
        entity.RetryCount = request.RetryCount;
        entity.DelayInSeconds = request.DelayInSeconds;
        entity.Priority = request.Priority;
        entity.NotificationTemplate = template;
        entity.ProviderConfiguration = provider;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToDetailResponse(entity);
    }

    public async Task<NotificationRuleDetailResponse> EnableNotificationRuleAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationRules
            .Include(r => r.EventDefinition)
            .Include(r => r.NotificationTemplate)
            .Include(r => r.ProviderConfiguration)
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationId == tenantId && !r.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification rule with id '{id}' was not found.");
        }

        await ValidateAndGetDependenciesAsync(
            tenantId,
            entity.EventDefinitionId,
            entity.NotificationTemplateId,
            entity.ProviderConfigurationId,
            entity.Channel);

        entity.IsEnabled = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToDetailResponse(entity);
    }

    public async Task<NotificationRuleDetailResponse> DisableNotificationRuleAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationRules
            .Include(r => r.EventDefinition)
            .Include(r => r.NotificationTemplate)
            .Include(r => r.ProviderConfiguration)
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationId == tenantId && !r.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification rule with id '{id}' was not found.");
        }

        entity.IsEnabled = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToDetailResponse(entity);
    }

    public async Task DeleteNotificationRuleAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationRules
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationId == tenantId && !r.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification rule with id '{id}' was not found.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();
    }

    private Guid GetApplicationAdminTenantId()
    {
        if (!_currentUser.IsApplicationAdmin || !_currentUser.ApplicationId.HasValue)
        {
            throw new ForbiddenException("Only ApplicationAdmin users with an assigned ApplicationId can manage notification rules.");
        }

        return _currentUser.ApplicationId.Value;
    }

    private static void ValidateRuleParameters(int retryCount, int delayInSeconds, int priority, NotificationChannel channel)
    {
        if (retryCount < 0)
        {
            throw new BadRequestException("RetryCount must be greater than or equal to 0.");
        }

        if (delayInSeconds < 0)
        {
            throw new BadRequestException("DelayInSeconds must be greater than or equal to 0.");
        }

        if (priority < 0)
        {
            throw new BadRequestException("Priority must be greater than or equal to 0.");
        }

        if (!Enum.IsDefined(typeof(NotificationChannel), channel))
        {
            throw new BadRequestException("Invalid notification channel.");
        }
    }

    private static void ValidateCompatibility(ProviderType providerType, NotificationChannel channel, NotificationChannel templateChannel)
    {
        if (templateChannel != channel)
        {
            throw new BadRequestException($"Notification template channel '{templateChannel}' does not match rule channel '{channel}'.");
        }

        var isCompatible = (providerType, channel) switch
        {
            (ProviderType.Smtp, NotificationChannel.Email) => true,
            (ProviderType.TwilioSms, NotificationChannel.Sms) => true,
            (ProviderType.TwilioWhatsApp, NotificationChannel.WhatsApp) => true,
            _ => false
        };

        if (!isCompatible)
        {
            throw new BadRequestException($"ProviderType '{providerType}' is not compatible with Channel '{channel}'.");
        }
    }

    private async Task<(EventDefinition eventDef, NotificationTemplate template, ProviderConfiguration provider)> ValidateAndGetDependenciesAsync(
        Guid tenantId,
        Guid eventDefId,
        Guid templateId,
        Guid providerId,
        NotificationChannel channel)
    {
        var eventDef = await _dbContext.EventDefinitions
            .FirstOrDefaultAsync(e => e.Id == eventDefId && !e.IsDeleted);

        if (eventDef == null)
        {
            throw new NotFoundException($"Event definition with id '{eventDefId}' was not found.");
        }

        if (eventDef.ApplicationId != tenantId)
        {
            throw new ForbiddenException("Event definition does not belong to your application.");
        }

        if (!eventDef.IsActive)
        {
            throw new BadRequestException("Referenced event definition is not active.");
        }

        var template = await _dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && !t.IsDeleted);

        if (template == null)
        {
            throw new NotFoundException($"Notification template with id '{templateId}' was not found.");
        }

        if (template.ApplicationId != tenantId)
        {
            throw new ForbiddenException("Notification template does not belong to your application.");
        }

        if (!template.IsActive)
        {
            throw new BadRequestException("Referenced notification template is not active.");
        }

        var provider = await _dbContext.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Id == providerId && !p.IsDeleted);

        if (provider == null)
        {
            throw new NotFoundException($"Provider configuration with id '{providerId}' was not found.");
        }

        if (provider.ApplicationId != tenantId)
        {
            throw new ForbiddenException("Provider configuration does not belong to your application.");
        }

        if (!provider.IsEnabled)
        {
            throw new BadRequestException("Referenced provider configuration is not enabled.");
        }

        ValidateCompatibility(provider.ProviderType, channel, template.Channel);

        return (eventDef, template, provider);
    }

    private static NotificationRuleResponse MapToSummaryResponse(NotificationRule entity)
    {
        return new NotificationRuleResponse
        {
            Id = entity.Id,
            EventDefinitionId = entity.EventDefinitionId,
            EventDefinitionCode = entity.EventDefinition?.Code ?? string.Empty,
            EventDefinitionName = entity.EventDefinition?.Name ?? string.Empty,
            Channel = entity.Channel.ToString(),
            NotificationTemplateId = entity.NotificationTemplateId,
            TemplateVersion = entity.NotificationTemplate?.Version ?? 0,
            ProviderConfigurationId = entity.ProviderConfigurationId,
            ProviderName = entity.ProviderConfiguration?.DisplayName ?? string.Empty,
            RetryCount = entity.RetryCount,
            DelayInSeconds = entity.DelayInSeconds,
            Priority = entity.Priority,
            IsEnabled = entity.IsEnabled,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static NotificationRuleDetailResponse MapToDetailResponse(NotificationRule entity)
    {
        return new NotificationRuleDetailResponse
        {
            Id = entity.Id,
            ApplicationId = entity.ApplicationId,
            EventDefinitionId = entity.EventDefinitionId,
            NotificationTemplateId = entity.NotificationTemplateId,
            ProviderConfigurationId = entity.ProviderConfigurationId,
            Channel = entity.Channel.ToString(),
            RetryCount = entity.RetryCount,
            DelayInSeconds = entity.DelayInSeconds,
            Priority = entity.Priority,
            IsEnabled = entity.IsEnabled,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId,
            UpdatedAt = entity.UpdatedAt,
            UpdatedByUserId = entity.UpdatedByUserId,
            EventDefinition = new EventDefinitionSummaryDto
            {
                Id = entity.EventDefinition.Id,
                Code = entity.EventDefinition.Code,
                Name = entity.EventDefinition.Name
            },
            NotificationTemplate = new NotificationTemplateSummaryDto
            {
                Id = entity.NotificationTemplate.Id,
                Channel = entity.NotificationTemplate.Channel.ToString(),
                Subject = entity.NotificationTemplate.Subject,
                Body = entity.NotificationTemplate.Body,
                Version = entity.NotificationTemplate.Version,
                IsActive = entity.NotificationTemplate.IsActive
            },
            ProviderConfiguration = new ProviderConfigurationSummaryDto
            {
                Id = entity.ProviderConfiguration.Id,
                ProviderType = entity.ProviderConfiguration.ProviderType.ToString(),
                DisplayName = entity.ProviderConfiguration.DisplayName,
                IsEnabled = entity.ProviderConfiguration.IsEnabled
            }
        };
    }
}
