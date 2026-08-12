using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Events;
using EventFlow.Application.Interfaces.Notifications;
using EventFlow.Infrastructure.Configuration;
using EventFlow.Infrastructure.Persistence;
using EventFlow.Infrastructure.Services;
using EventFlow.Infrastructure.Services.Authentication;
using EventFlow.Worker.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddDbContext<EventFlowDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IEventProcessor, EventProcessor>();
        services.AddScoped<INotificationProcessor, NotificationProcessor>();

        return services;
    }
}