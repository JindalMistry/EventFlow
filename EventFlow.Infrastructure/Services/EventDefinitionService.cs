using EventFlow.Application.Common;
using EventFlow.Application.DTOs.EventDefinitions;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class EventDefinitionService : IEventDefinitionService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public EventDefinitionService(EventFlowDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<EventDefinitionResponse> CreateEventDefinitionAsync(CreateEventDefinitionRequest request)
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
                throw new ForbiddenException("ApplicationAdmin can only create event definitions for their own application.");
            }

            targetApplicationId = _currentUser.ApplicationId.Value;
        }

        if (targetApplicationId == Guid.Empty)
        {
            throw new BadRequestException("ApplicationId is required.");
        }

        var trimmedCode = request.Code?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedCode))
        {
            throw new BadRequestException("Code is required.");
        }

        var trimmedName = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new BadRequestException("Name is required.");
        }

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == targetApplicationId && !a.IsDeleted);

        if (application == null)
        {
            throw new NotFoundException($"Application with id '{targetApplicationId}' was not found.");
        }

        if (application.Status != ApplicationStatus.Active)
        {
            throw new BadRequestException("Cannot create event definition for an inactive application.");
        }

        var duplicateExists = await _dbContext.EventDefinitions
            .AnyAsync(e => e.ApplicationId == targetApplicationId &&
                           e.Code.ToLower() == trimmedCode.ToLower());

        if (duplicateExists)
        {
            throw new BadRequestException($"An event definition with code '{trimmedCode}' already exists for this application.");
        }

        var now = DateTime.UtcNow;

        var entity = new EventDefinition
        {
            Id = Guid.NewGuid(),
            ApplicationId = targetApplicationId,
            Code = trimmedCode,
            Name = trimmedName,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            CreatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null
        };

        _dbContext.EventDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<PagedResponse<EventDefinitionResponse>> GetEventDefinitionsAsync(EventDefinitionQueryParameters query)
    {
        var queryable = _dbContext.EventDefinitions
            .AsNoTracking()
            .Include(e => e.NotificationRules)
            .Where(e => !e.IsDeleted);

        if (_currentUser.IsApplicationAdmin)
        {
            if (!_currentUser.ApplicationId.HasValue)
            {
                throw new ForbiddenException("ApplicationAdmin user does not have an assigned ApplicationId.");
            }

            if (query.ApplicationId.HasValue && query.ApplicationId.Value != _currentUser.ApplicationId.Value)
            {
                throw new ForbiddenException("ApplicationAdmin can only view event definitions for their own application.");
            }

            queryable = queryable.Where(e => e.ApplicationId == _currentUser.ApplicationId.Value);
        }
        else if (_currentUser.IsRootAdmin)
        {
            if (query.ApplicationId.HasValue)
            {
                queryable = queryable.Where(e => e.ApplicationId == query.ApplicationId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(e => e.Code.ToLower().Contains(search) || e.Name.ToLower().Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(e => e.IsActive == query.IsActive.Value);
        }

        var totalCount = await queryable.CountAsync();
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var items = await queryable
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<EventDefinitionResponse>
        {
            Items = items.Select(MapToResponse),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<EventDefinitionResponse> GetEventDefinitionByIdAsync(Guid id)
    {
        var entity = await _dbContext.EventDefinitions
            .AsNoTracking()
            .Include(e => e.NotificationRules)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Event definition with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        return MapToResponse(entity);
    }

    public async Task<EventDefinitionResponse> UpdateEventDefinitionAsync(Guid id, UpdateEventDefinitionRequest request)
    {
        var entity = await _dbContext.EventDefinitions
            .Include(e => e.NotificationRules)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Event definition with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        var trimmedName = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new BadRequestException("Name is required.");
        }

        entity.Name = trimmedName;
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<EventDefinitionResponse> ActivateEventDefinitionAsync(Guid id)
    {
        var entity = await _dbContext.EventDefinitions
            .Include(e => e.NotificationRules)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Event definition with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(a => a.Id == entity.ApplicationId && !a.IsDeleted);

        if (application == null || application.Status != ApplicationStatus.Active)
        {
            throw new BadRequestException("Cannot activate event definition because the associated application is inactive or deleted.");
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<EventDefinitionResponse> DeactivateEventDefinitionAsync(Guid id)
    {
        var entity = await _dbContext.EventDefinitions
            .Include(e => e.NotificationRules)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Event definition with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task DeleteEventDefinitionAsync(Guid id)
    {
        var entity = await _dbContext.EventDefinitions
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException($"Event definition with id '{id}' was not found.");
        }

        ValidateTenantAccess(entity.ApplicationId);

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId : null;

        await _dbContext.SaveChangesAsync();
    }

    private void ValidateTenantAccess(Guid applicationId)
    {
        if (_currentUser.IsApplicationAdmin)
        {
            if (!_currentUser.ApplicationId.HasValue || _currentUser.ApplicationId.Value != applicationId)
            {
                throw new ForbiddenException("ApplicationAdmin can only access event definitions belonging to their own application.");
            }
        }
    }

    private static EventDefinitionResponse MapToResponse(EventDefinition entity)
    {
        return new EventDefinitionResponse
        {
            Id = entity.Id,
            ApplicationId = entity.ApplicationId,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            NotificationRuleCount = entity.NotificationRules?.Count(r => !r.IsDeleted) ?? 0,
            ActiveNotificationRuleCount = entity.NotificationRules?.Count(r => !r.IsDeleted && r.IsEnabled) ?? 0,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId,
            UpdatedAt = entity.UpdatedAt,
            UpdatedByUserId = entity.UpdatedByUserId
        };
    }
}
