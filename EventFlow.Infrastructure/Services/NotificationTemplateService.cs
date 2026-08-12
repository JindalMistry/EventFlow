using EventFlow.Application.Common;
using EventFlow.Application.DTOs.NotificationTemplates;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public NotificationTemplateService(EventFlowDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<NotificationTemplateResponse> CreateNotificationTemplateAsync(CreateNotificationTemplateRequest request)
    {
        var tenantId = GetApplicationAdminTenantId();

        if (request.EventDefinitionId == Guid.Empty)
        {
            throw new BadRequestException("EventDefinitionId is required.");
        }

        var eventDef = await _dbContext.EventDefinitions
            .FirstOrDefaultAsync(e => e.Id == request.EventDefinitionId && !e.IsDeleted);

        if (eventDef == null)
        {
            throw new NotFoundException($"Event definition with id '{request.EventDefinitionId}' was not found.");
        }

        if (eventDef.ApplicationId != tenantId)
        {
            throw new ForbiddenException("Event definition does not belong to your application.");
        }

        if (!eventDef.IsActive)
        {
            throw new BadRequestException("Cannot create notification template for an inactive event definition.");
        }

        if (!Enum.IsDefined(typeof(NotificationChannel), request.Channel))
        {
            throw new BadRequestException("Invalid notification channel.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new BadRequestException("Body is required.");
        }

        var trimmedSubject = request.Subject?.Trim();
        if (request.Channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(trimmedSubject))
        {
            throw new BadRequestException("Subject is required for Email channel.");
        }

        var maxVersion = await _dbContext.NotificationTemplates
            .Where(t => t.ApplicationId == tenantId &&
                        t.EventDefinitionId == request.EventDefinitionId &&
                        t.Channel == request.Channel)
            .Select(t => (int?)t.Version)
            .MaxAsync() ?? 0;

        var version = maxVersion + 1;
        var now = DateTime.UtcNow;

        var entity = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            ApplicationId = tenantId,
            EventDefinitionId = request.EventDefinitionId,
            Channel = request.Channel,
            Subject = trimmedSubject ?? string.Empty,
            Body = request.Body,
            IsActive = request.IsActive,
            Version = version,
            CreatedAt = now,
            CreatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null,
            EventDefinition = eventDef
        };

        _dbContext.NotificationTemplates.Add(entity);
        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<PagedResponse<NotificationTemplateResponse>> GetNotificationTemplatesAsync(NotificationTemplateQueryParameters query)
    {
        var tenantId = GetApplicationAdminTenantId();

        var queryable = _dbContext.NotificationTemplates
            .AsNoTracking()
            .Include(t => t.EventDefinition)
            .Where(t => t.ApplicationId == tenantId && !t.IsDeleted);

        if (query.EventDefinitionId.HasValue)
        {
            queryable = queryable.Where(t => t.EventDefinitionId == query.EventDefinitionId.Value);
        }

        if (query.Channel.HasValue)
        {
            queryable = queryable.Where(t => t.Channel == query.Channel.Value);
        }

        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(t => t.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(t => t.Subject.ToLower().Contains(search) || t.Body.ToLower().Contains(search));
        }

        var totalCount = await queryable.CountAsync();
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var items = await queryable
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<NotificationTemplateResponse>
        {
            Items = items.Select(MapToResponse),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<NotificationTemplateResponse> GetNotificationTemplateByIdAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationTemplates
            .AsNoTracking()
            .Include(t => t.EventDefinition)
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationId == tenantId && !t.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification template with id '{id}' was not found.");
        }

        return MapToResponse(entity);
    }

    public async Task<NotificationTemplateResponse> GetTemplateById(Guid Id, CancellationToken Token)
    {
        var entity = await _dbContext.NotificationTemplates
            .AsNoTracking()
            .Include(t => t.EventDefinition)
            .FirstOrDefaultAsync(t => t.Id == Id && !t.IsDeleted, Token);

        if (entity == null)
        {
            throw new NotFoundException($"Notification does not exist.");
        }

        return MapToResponse(entity);
    }

    public async Task<NotificationTemplateResponse> UpdateNotificationTemplateAsync(Guid id, UpdateNotificationTemplateRequest request)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationTemplates
            .Include(t => t.EventDefinition)
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationId == tenantId && !t.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification template with id '{id}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new BadRequestException("Body is required.");
        }

        var trimmedSubject = request.Subject?.Trim();
        if (entity.Channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(trimmedSubject))
        {
            throw new BadRequestException("Subject is required for Email channel.");
        }

        entity.Subject = trimmedSubject ?? string.Empty;
        entity.Body = request.Body;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<NotificationTemplateResponse> ActivateNotificationTemplateAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationTemplates
            .Include(t => t.EventDefinition)
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationId == tenantId && !t.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification template with id '{id}' was not found.");
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<NotificationTemplateResponse> DeactivateNotificationTemplateAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationTemplates
            .Include(t => t.EventDefinition)
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationId == tenantId && !t.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification template with id '{id}' was not found.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task DeleteNotificationTemplateAsync(Guid id)
    {
        var tenantId = GetApplicationAdminTenantId();

        var entity = await _dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationId == tenantId && !t.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Notification template with id '{id}' was not found.");
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
            throw new ForbiddenException("Only ApplicationAdmin users with an assigned ApplicationId can manage notification templates.");
        }

        return _currentUser.ApplicationId.Value;
    }

    private static NotificationTemplateResponse MapToResponse(NotificationTemplate entity)
    {
        return new NotificationTemplateResponse
        {
            Id = entity.Id,
            ApplicationId = entity.ApplicationId,
            EventDefinitionId = entity.EventDefinitionId,
            EventDefinitionCode = entity.EventDefinition?.Code,
            Channel = entity.Channel.ToString(),
            Subject = entity.Subject,
            Body = entity.Body,
            IsActive = entity.IsActive,
            Version = entity.Version,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId,
            UpdatedAt = entity.UpdatedAt,
            UpdatedByUserId = entity.UpdatedByUserId
        };
    }
}
