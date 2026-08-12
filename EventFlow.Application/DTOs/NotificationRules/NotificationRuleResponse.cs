namespace EventFlow.Application.DTOs.NotificationRules;

public class NotificationRuleResponse
{
    public Guid Id { get; set; }
    public Guid EventDefinitionId { get; set; }
    public string EventDefinitionCode { get; set; } = string.Empty;
    public string EventDefinitionName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public Guid NotificationTemplateId { get; set; }
    public int TemplateVersion { get; set; }
    public Guid ProviderConfigurationId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public int DelayInSeconds { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
