using EventFlow.Domain.Common;
using EventFlow.Domain.Enums;

namespace EventFlow.Domain.Entities;

public class NotificationRule : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public Guid EventDefinitionId { get; set; }
    public Guid NotificationTemplateId { get; set; }
    public Guid ProviderConfigurationId { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; }
    public int RetryCount { get; set; }
    public int DelayInSeconds { get; set; }
    public int Priority { get; set; }

    public Application Application { get; set; } = null!;
    public EventDefinition EventDefinition { get; set; } = null!;
    public NotificationTemplate NotificationTemplate { get; set; } = null!;
    public ProviderConfiguration ProviderConfiguration { get; set; } = null!;
}
