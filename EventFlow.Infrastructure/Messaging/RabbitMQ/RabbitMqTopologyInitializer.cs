using Microsoft.Extensions.Hosting;

namespace EventFlow.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqTopologyInitializer : IHostedService
{
    private readonly IRabbitMqConnection _connection;

    public RabbitMqTopologyInitializer(IRabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var channel =
            await _connection.CreateChannelAsync(cancellationToken);

        await RabbitMqTopology.ConfigureEventAsync(
            channel,
            cancellationToken);

        await RabbitMqTopology.ConfigureNotificationAsync(
                channel,
                cancellationToken
            );

        await RabbitMqTopology.ConfigureRetryAsync( 
            channel, 
            cancellationToken );

        await RabbitMqTopology.CreateNotificationDeadLetterQueueAsync( 
            channel, 
            cancellationToken );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}