namespace EventFlow.Application.DTOs.NotificationTemplates;

public class NotificationTemplateResponse
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid EventDefinitionId { get; set; }
    public string? EventDefinitionCode { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
