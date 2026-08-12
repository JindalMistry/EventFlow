using EventFlow.Application.DTOs.Messaging;

namespace EventFlow.Application.Interfaces.Messaging;

public interface INotificationDeadLetterMessagePublisher
{
    Task PublishAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default);
}