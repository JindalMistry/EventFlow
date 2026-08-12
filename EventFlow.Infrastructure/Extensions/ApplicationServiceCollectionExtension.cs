using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Notifications;
using EventFlow.Application.Interfaces.Providers;
using EventFlow.Infrastructure.Configuration;
using EventFlow.Infrastructure.Providers.WhatsApp;
using EventFlow.Infrastructure.Services;
using EventFlow.Infrastructure.Services.Authentication;
using EventFlow.Infrastructure.Services.Providers;
using EventFlow.Worker.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Infrastructure.Extensions
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IProviderTestService, ProviderTestService>();
            services.AddScoped<IProviderConfigurationValidator, ProviderConfigurationValidator>();
            services.AddScoped<IProviderConfigurationService, ProviderConfigurationService>();
            services.AddScoped<IEventDefinitionService, EventDefinitionService>();
            services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
            services.AddScoped<IPublishedEventService, PublishedEventService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationAttemptService, NotificationAttemptService>();
            services.AddScoped<INotificationRuleService, NotificationRuleService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ITemplateEngine, TemplateEngine>();
            services.AddScoped<INotificationProviderFactory, NotificationProviderFactory>();
            services.AddScoped<IEmailProvider, SmtpEmailProvider>();
            services.AddScoped<ISmsProvider, TwilioSmsProvider>();
            services.AddScoped<IWhatsAppProvider, TwilioWhatsAppProvider>();


            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

            return services;
        }
    }
}
