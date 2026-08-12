namespace EventFlow.Application.DTOs.NotificationTemplates;

public class UpdateNotificationTemplateRequest
{
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
}
