using System.Text;
using System.Text.Json;
using EventFlow.Application.Interfaces.Messaging;
using EventFlow.Application.Messaging;
using RabbitMQ.Client;

namespace EventFlow.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqEventMessagePublisher : IEventMessagePublisher
{
    private readonly IRabbitMqConnection _connection;

    public RabbitMqEventMessagePublisher(IRabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync(
        EventMessage message,
        CancellationToken cancellationToken = default)
    {
        await using var channel =
            await _connection.CreateChannelAsync(cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqTopology.EventExchange,
            routingKey: RabbitMqTopology.EventRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}