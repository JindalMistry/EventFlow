using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;

namespace EventFlow.Application.Interfaces;

public interface IPublishedEventService
{
    Task<PublishedEvent> CreateAsync(PublishedEvent publishedEvent, CancellationToken cancellationToken = default);
    Task<PublishedEvent> CreateAsync(
        Guid applicationId,
        Guid eventDefinitionId,
        Guid correlationId,
        string payload,
        DateTime occurredAt,
        EventStatus status = EventStatus.Received,
        CancellationToken cancellationToken = default);
    Task<PublishedEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CompleteProcessingAsync(Guid id, EventStatus status, DateTime? processedAt = null, CancellationToken cancellationToken = default);

    Task UpdatePublishedEventStatusAsync(Guid Id, EventStatus status, CancellationToken cancellationToken);
}
