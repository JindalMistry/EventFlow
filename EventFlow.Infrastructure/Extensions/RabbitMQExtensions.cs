using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Events;
using EventFlow.Application.Interfaces.Messaging;
using EventFlow.Infrastructure.Messaging.RabbitMQ;
using EventFlow.Infrastructure.Services;
using EventFlow.Worker.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Infrastructure.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMQ"));

        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddScoped<IEventMessagePublisher, RabbitMqEventMessagePublisher>();
        services.AddScoped<INotificationMessagePublisher, RabbitMqNotificationMessagePublisher>();
        services.AddScoped<IRetryMessagePublisher, RabbitMqRetryMessagePublisher>();
        services.AddScoped<INotificationDeadLetterMessagePublisher, RabbitMqNotificationDLMessagePublisher>();
        services.AddHostedService<RabbitMqTopologyInitializer>();

        return services;
    }
}