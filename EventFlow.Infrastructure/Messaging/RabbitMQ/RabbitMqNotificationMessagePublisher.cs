using EventFlow.Application.DTOs.Messaging;
using EventFlow.Application.Interfaces.Messaging;
using EventFlow.Application.Messaging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventFlow.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqNotificationMessagePublisher : INotificationMessagePublisher
{
    private readonly IRabbitMqConnection _connection;

    public RabbitMqNotificationMessagePublisher(IRabbitMqConnection connection)
    {
        _connection = connection;
    }
    public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
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
            exchange: RabbitMqTopology.NotificationExchange,
            routingKey: RabbitMqTopology.NotificationRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}

