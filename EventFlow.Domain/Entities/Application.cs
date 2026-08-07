using EventFlow.Domain.Common;
using EventFlow.Domain.Enums;

namespace EventFlow.Domain.Entities;

public class Application : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SupportEmail { get; set; }
    public string ApiKeyHash { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime ApiKeyCreatedAt { get; set; }
    public DateTime? ApiKeyExpiresAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ProviderConfiguration> ProviderConfigurations { get; set; } = new List<ProviderConfiguration>();
    public ICollection<EventDefinition> EventDefinitions { get; set; } = new List<EventDefinition>();
    public ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();
    public ICollection<NotificationRule> NotificationRules { get; set; } = new List<NotificationRule>();
    public ICollection<PublishedEvent> PublishedEvents { get; set; } = new List<PublishedEvent>();
}
