using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.NotificationRules;

public class UpdateNotificationRuleRequest
{
    public Guid NotificationTemplateId { get; set; }
    public Guid ProviderConfigurationId { get; set; }
    public NotificationChannel Channel { get; set; }
    public int RetryCount { get; set; }
    public int DelayInSeconds { get; set; }
    public int Priority { get; set; }
}
