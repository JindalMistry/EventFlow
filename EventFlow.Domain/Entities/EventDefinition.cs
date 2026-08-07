using EventFlow.Domain.Common;

namespace EventFlow.Domain.Entities;

public class EventDefinition : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public Application Application { get; set; } = null!;
    public ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();
    public ICollection<NotificationRule> NotificationRules { get; set; } = new List<NotificationRule>();
    public ICollection<PublishedEvent> PublishedEvents { get; set; } = new List<PublishedEvent>();
}
