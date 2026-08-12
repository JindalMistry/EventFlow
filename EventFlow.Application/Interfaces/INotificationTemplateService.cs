using EventFlow.Application.Common;
using EventFlow.Application.DTOs.NotificationTemplates;

namespace EventFlow.Application.Interfaces;

public interface INotificationTemplateService
{
    Task<NotificationTemplateResponse> CreateNotificationTemplateAsync(CreateNotificationTemplateRequest request);
    Task<PagedResponse<NotificationTemplateResponse>> GetNotificationTemplatesAsync(NotificationTemplateQueryParameters query);
    Task<NotificationTemplateResponse> GetNotificationTemplateByIdAsync(Guid id);
    Task<NotificationTemplateResponse> UpdateNotificationTemplateAsync(Guid id, UpdateNotificationTemplateRequest request);
    Task<NotificationTemplateResponse> ActivateNotificationTemplateAsync(Guid id);
    Task<NotificationTemplateResponse> DeactivateNotificationTemplateAsync(Guid id);
    Task DeleteNotificationTemplateAsync(Guid id);
    Task<NotificationTemplateResponse> GetTemplateById(Guid Id, CancellationToken Token);
}
