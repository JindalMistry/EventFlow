using EventFlow.Application.Messaging;

namespace EventFlow.Application.Interfaces.Events;

public interface IEventProcessor
{
    Task ProcessAsync(
        EventMessage message,
        CancellationToken cancellationToken = default);
}