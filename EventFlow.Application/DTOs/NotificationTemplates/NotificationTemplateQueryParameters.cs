using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.NotificationTemplates;

public class NotificationTemplateQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public Guid? EventDefinitionId { get; set; }
    public NotificationChannel? Channel { get; set; }
    public bool? IsActive { get; set; }
}
