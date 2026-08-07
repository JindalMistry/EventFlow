using EventFlow.Domain.Common;
using EventFlow.Domain.Enums;

namespace EventFlow.Domain.Entities;

public class NotificationTemplate : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public Guid EventDefinitionId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Version { get; set; }

    public Application Application { get; set; } = null!;
    public EventDefinition EventDefinition { get; set; } = null!;
}
