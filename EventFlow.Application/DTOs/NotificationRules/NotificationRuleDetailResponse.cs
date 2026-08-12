namespace EventFlow.Application.DTOs.NotificationRules;

public class EventDefinitionSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class NotificationTemplateSummaryDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Version { get; set; }
    public bool IsActive { get; set; }
}

public class ProviderConfigurationSummaryDto
{
    public Guid Id { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class NotificationRuleDetailResponse
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid EventDefinitionId { get; set; }
    public Guid NotificationTemplateId { get; set; }
    public Guid ProviderConfigurationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public int DelayInSeconds { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public EventDefinitionSummaryDto EventDefinition { get; set; } = null!;
    public NotificationTemplateSummaryDto NotificationTemplate { get; set; } = null!;
    public ProviderConfigurationSummaryDto ProviderConfiguration { get; set; } = null!;
}
