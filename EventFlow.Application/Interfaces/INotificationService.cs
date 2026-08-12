using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;

namespace EventFlow.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<Notification> CreateAsync(
        Guid eventId,
        Guid notificationRuleId,
        Guid notificationTemplateId,
        Guid providerConfigurationId,
        NotificationChannel channel,
        string recipient,
        NotificationStatus status = NotificationStatus.Pending,
        DateTime? queuedAt = null,
        CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CompleteProcessingAsync(Guid id, NotificationStatus status, string? failureReason = null, DateTime? sentAt = null, CancellationToken cancellationToken = default);

    Task<Notification?> GetByEventAndRuleAsync(
        Guid eventId,
        Guid notificationRuleId,
        CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Guid Id, NotificationStatus Status, CancellationToken Token);
}
