using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;

namespace EventFlow.Application.Interfaces;

public interface INotificationAttemptService
{
    Task<NotificationAttempt> CreateAsync(NotificationAttempt attempt, CancellationToken cancellationToken = default);
    Task<NotificationAttempt> CreateAsync(
        Guid notificationId,
        int attemptNumber,
        AttemptStatus status = AttemptStatus.Retrying,
        string? providerResponse = null,
        DateTime? startedAt = null,
        CancellationToken cancellationToken = default);
    Task<NotificationAttempt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CompleteAsync(
        Guid id, 
        AttemptStatus status, 
        string? errorMessage = null, 
        string? providerResponse = null, 
        DateTime? completedAt = null, 
        CancellationToken cancellationToken = default);

    Task<NotificationAttempt?> GetLatestAttempByNotificationId(
        Guid NotificationId,
        CancellationToken cancellationToken= default);
}
