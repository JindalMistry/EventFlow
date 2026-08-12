using RabbitMQ.Client;

namespace EventFlow.Infrastructure.Messaging.RabbitMQ;

public static class RabbitMqTopology
{
    public const string EventExchange = "events";
    public const string EventQueue = "event.processing";
    public const string EventRoutingKey = "event.process";

    public const string NotificationExchange = "notifications";
    public const string NotificationQueue = "notification.processing";
    public const string NotificationRoutingKey = "notification.process";

    public const string RetryExchange = "retry";
    public const string RetryQueue = "retry.processing";
    public const string RetryRoutingKey = "retry.process";

    private const int RetryTtlMilliseconds = 10_000;

    public const string NotificationDLX = "notifications.dlx";
    public const string NotificationDLQ = "notification.dlq";
    public const string NotificationDLXRoutingKey = "notifications.dead";

    public static async Task ConfigureEventAsync(
        IChannel channel,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: EventExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: EventQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: EventQueue,
            exchange: EventExchange,
            routingKey: EventRoutingKey,
            cancellationToken: cancellationToken);
    }

    public static async Task ConfigureNotificationAsync(
        IChannel channel,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: NotificationExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: NotificationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: NotificationQueue,
            exchange: NotificationExchange,
            routingKey: NotificationRoutingKey,
            cancellationToken: cancellationToken);
    }

    public static async Task ConfigureRetryAsync(
        IChannel channel,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: RetryExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var retryQueueArguments = new Dictionary<string, object?>
        {
            // Message stays here for 10 seconds.
            ["x-message-ttl"] = RetryTtlMilliseconds,

            // When TTL expires, send the message
            // to the Notification Exchange.
            ["x-dead-letter-exchange"] = NotificationExchange,

            // Route it back to the normal Notification Queue.
            ["x-dead-letter-routing-key"] = NotificationRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryQueueArguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: RetryQueue,
            exchange: RetryExchange,
            routingKey: RetryRoutingKey,
            cancellationToken: cancellationToken);
    }

    public static async Task CreateNotificationDeadLetterQueueAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: NotificationDLX,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: NotificationDLQ,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: NotificationDLQ,
            exchange: NotificationDLX,
            routingKey: NotificationDLXRoutingKey,
            cancellationToken: cancellationToken);
    }
}