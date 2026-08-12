namespace EventFlow.Application.DTOs.EventDefinitions;

public class EventDefinitionResponse
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int NotificationRuleCount { get; set; }
    public int ActiveNotificationRuleCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
