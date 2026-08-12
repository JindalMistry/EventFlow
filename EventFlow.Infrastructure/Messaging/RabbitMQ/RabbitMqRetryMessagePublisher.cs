using EventFlow.Application.DTOs.Messaging;
using EventFlow.Application.Interfaces.Messaging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventFlow.Infrastructure.Messaging.RabbitMQ
{
    public sealed class RabbitMqRetryMessagePublisher: IRetryMessagePublisher
    {
        private readonly IRabbitMqConnection _connection;

        public RabbitMqRetryMessagePublisher(IRabbitMqConnection connection)
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
                exchange: RabbitMqTopology.RetryExchange,
                routingKey: RabbitMqTopology.RetryRoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
