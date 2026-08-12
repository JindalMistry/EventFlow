using System.Text.Json;
using EventFlow.Application.Interfaces.Events;
using EventFlow.Application.Messaging;
using EventFlow.Infrastructure.Messaging.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventFlow.Worker.Consumers;

public sealed class EventConsumer : BackgroundService
{
    private readonly IRabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventConsumer> _logger;

    public EventConsumer(
        IRabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<EventConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
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
                var message = JsonSerializer.Deserialize<EventMessage>(
                    args.Body.Span);

                if (message is null)
                {
                    _logger.LogWarning(
                        "Received an invalid EventMessage.");

                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }

                _logger.LogInformation(
                    "Processing event. PublishedEventId: {PublishedEventId}, CorrelationId: {CorrelationId}",
                    message.PublishedEventId,
                    message.CorrelationId);

                await using var scope = _scopeFactory.CreateAsyncScope();

                var eventProcessor = scope.ServiceProvider.GetRequiredService<IEventProcessor>();

                await eventProcessor.ProcessAsync(message, stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);

                _logger.LogInformation(
                    "Event processed successfully. PublishedEventId: {PublishedEventId}",
                    message.PublishedEventId);
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
            queue: RabbitMqTopology.EventQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Event consumer started. Listening on {Queue}",
            RabbitMqTopology.EventQueue);

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
    }
}