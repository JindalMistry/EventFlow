using EventFlow.Domain.Common;
using EventFlow.Domain.Enums;

namespace EventFlow.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid NotificationRuleId { get; set; }
    public Guid NotificationTemplateId { get; set; }
    public Guid ProviderConfigurationId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }

    public PublishedEvent Event { get; set; } = null!;
    public NotificationRule NotificationRule { get; set; } = null!;
    public NotificationTemplate NotificationTemplate { get; set; } = null!;
    public ProviderConfiguration ProviderConfiguration { get; set; } = null!;
    public ICollection<NotificationAttempt> NotificationAttempts { get; set; } = new List<NotificationAttempt>();
}
