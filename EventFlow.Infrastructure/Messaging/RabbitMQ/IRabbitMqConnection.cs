using RabbitMQ.Client;

namespace EventFlow.Infrastructure.Messaging.RabbitMQ;

public interface IRabbitMqConnection : IAsyncDisposable
{
    Task<IChannel> CreateChannelAsync(
        CancellationToken cancellationToken = default);
}