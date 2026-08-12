using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class PublishedEventService : IPublishedEventService
{
    private readonly EventFlowDbContext _dbContext;

    public PublishedEventService(EventFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PublishedEvent> CreateAsync(PublishedEvent publishedEvent, CancellationToken cancellationToken = default)
    {
        if (publishedEvent == null)
        {
            throw new BadRequestException("Published event cannot be null.");
        }

        if (publishedEvent.ApplicationId == Guid.Empty)
        {
            throw new BadRequestException("ApplicationId is required.");
        }

        if (publishedEvent.EventDefinitionId == Guid.Empty)
        {
            throw new BadRequestException("EventDefinitionId is required.");
        }

        if (publishedEvent.CorrelationId == Guid.Empty)
        {
            throw new BadRequestException("CorrelationId is required.");
        }

        if (string.IsNullOrWhiteSpace(publishedEvent.Payload))
        {
            throw new BadRequestException("Payload is required.");
        }

        if (publishedEvent.Id == Guid.Empty)
        {
            publishedEvent.Id = Guid.NewGuid();
        }

        if (publishedEvent.OccurredAt == default)
        {
            publishedEvent.OccurredAt = DateTime.UtcNow;
        }

        if (publishedEvent.CreatedAt == default)
        {
            publishedEvent.CreatedAt = DateTime.UtcNow;
        }

        _dbContext.PublishedEvents.Add(publishedEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return publishedEvent;
    }

    public async Task<PublishedEvent> CreateAsync(
        Guid applicationId,
        Guid eventDefinitionId,
        Guid correlationId,
        string payload,
        DateTime occurredAt,
        EventStatus status = EventStatus.Received,
        CancellationToken cancellationToken = default)
    {
        var publishedEvent = new PublishedEvent
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            EventDefinitionId = eventDefinitionId,
            CorrelationId = correlationId,
            Payload = payload,
            Status = status,
            OccurredAt = occurredAt == default ? DateTime.UtcNow : occurredAt,
            CreatedAt = DateTime.UtcNow
        };

        return await CreateAsync(publishedEvent, cancellationToken);
    }

    public async Task<PublishedEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.PublishedEvents
            .AsNoTracking()
            .Include(e => e.EventDefinition)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task CompleteProcessingAsync(
        Guid id,
        EventStatus status,
        DateTime? processedAt = null,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("Published event ID is required.");
        }

        var entity = await _dbContext.PublishedEvents
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Published event with id '{id}' was not found.");
        }

        entity.Status = status;
        entity.ProcessedAt = processedAt ?? DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePublishedEventStatusAsync(Guid Id, EventStatus status, CancellationToken cancellationToken)
    {
        if (Id == Guid.Empty)
        {
            throw new BadRequestException("Published event ID is required.");
        }

        var entity = await _dbContext.PublishedEvents
            .FirstOrDefaultAsync(e => e.Id == Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Published event with id '{Id}' was not found.");
        }

        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
