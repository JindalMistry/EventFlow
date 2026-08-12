using EventFlow.Application.DTOs.Messaging;
using EventFlow.Application.Interfaces.Events;
using EventFlow.Application.Interfaces.Notifications;
using EventFlow.Application.Messaging;
using EventFlow.Infrastructure.Messaging.RabbitMQ;
using EventFlow.Infrastructure.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventFlow.Worker.Consumers
{
    public class NotificationConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationConsumer> _logger;

        public NotificationConsumer(
            IRabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationConsumer> logger)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var channel = 
                await _connection.CreateChannelAsync(stoppingToken);

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<NotificationMessage>(
                        args.Body.Span);

                    if (message is null)
                    {
                        _logger.LogWarning(
                            "Received an invalid NotificationMessage.");

                        await channel.BasicNackAsync(
                            deliveryTag: args.DeliveryTag,
                            multiple: false,
                            requeue: false);

                        return;
                    }

                    _logger.LogInformation(
                        "Processing Notification. NotificationId: {NotificationId}", message.NotificationId);

                    await using var scope = _scopeFactory.CreateAsyncScope();

                    var notificationProcessor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();

                    await notificationProcessor.ProcessAsync(message, stoppingToken);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);

                    _logger.LogInformation(
                        "Notification processed successfully. NotificationId: {NotificationId}",
                        message.NotificationId);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    // Application is shutting down.
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process RabbitMQ event message.");

                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false);
                }
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMqTopology.NotificationQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "Notification consumer started. Listening on {Queue}",
                RabbitMqTopology.NotificationQueue);

            try
            {
                await Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
            }

            throw new NotImplementedException();
        }
    }
}
