using EventFlow.Application.Messaging;

namespace EventFlow.Application.Interfaces.Messaging;

public interface IEventMessagePublisher
{
    Task PublishAsync(
        EventMessage message,
        CancellationToken cancellationToken = default);
}