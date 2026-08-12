using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.NotificationTemplates;

public class CreateNotificationTemplateRequest
{
    public Guid EventDefinitionId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
