using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.DTOs.TemplateEngine;
using EventFlow.Application.Interfaces.Providers;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventFlow.Infrastructure.Services.Providers;

public sealed class TwilioSmsProvider : ISmsProvider
{
    private readonly ILogger<TwilioSmsProvider> _logger;

    public TwilioSmsProvider(
        ILogger<TwilioSmsProvider> logger)
    {
        _logger = logger;
    }

    public async Task<ProviderSendResult> SendAsync(
        Notification notification,
        RenderedTemplate template,
        ProviderConfigurationResponse configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var twilioConfiguration =
                configuration.Configuration.Deserialize<TwilioSmsProviderConfigurationDto>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (twilioConfiguration is null)
            {
                return new ProviderSendResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Twilio SMS provider configuration is invalid."
                };
            }

            _logger.LogInformation(
                "Mock Twilio SMS provider processing notification {NotificationId}.",
                notification.Id);

            if (Random.Shared.NextDouble() < 0.5)
            {
                _logger.LogWarning(
                    "Mock Twilio SMS intentionally failed. NotificationId: {NotificationId}",
                    notification.Id);

                throw new InvalidOperationException(
                    "Mock Twilio SMS provider failed intentionally.");
            }

            // Simulate external provider/network latency.
            await Task.Delay(
                TimeSpan.FromSeconds(5),
                cancellationToken);

            _logger.LogInformation(
                "Mock Twilio SMS sent successfully to {Recipient}. NotificationId: {NotificationId}",
                notification.Recipient,
                notification.Id);

            return new ProviderSendResult
            {
                IsSuccess = true,
                ProviderResponse = "Mock Twilio SMS sent successfully."
            };
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Mock Twilio SMS provider failed. NotificationId: {NotificationId}",
                notification.Id);

            return new ProviderSendResult
            {
                IsSuccess = false,
                ProviderResponse = "Failed to send SMS",
                ErrorMessage = ex.Message
            };
        }
    }
}