using EventFlow.Domain.Common;
using EventFlow.Domain.Enums;

namespace EventFlow.Domain.Entities;

public class PublishedEvent : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public Guid EventDefinitionId { get; set; }
    public Guid CorrelationId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public EventStatus Status { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public Application Application { get; set; } = null!;
    public EventDefinition EventDefinition { get; set; } = null!;
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
