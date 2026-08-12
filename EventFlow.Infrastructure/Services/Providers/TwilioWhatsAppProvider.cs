using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.DTOs.TemplateEngine;
using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Providers;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventFlow.Infrastructure.Providers.WhatsApp;

public sealed class TwilioWhatsAppProvider : IWhatsAppProvider
{
    private readonly ILogger<TwilioWhatsAppProvider> _logger;

    public TwilioWhatsAppProvider(
        ILogger<TwilioWhatsAppProvider> logger)
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
                configuration.Configuration.Deserialize<TwilioWhatsAppProviderConfigurationDto>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (twilioConfiguration is null)
            {
                return new ProviderSendResult
                {
                    IsSuccess = false,
                    ErrorMessage =
                        "Twilio WhatsApp provider configuration is invalid."
                };
            }

            _logger.LogInformation(
                "Mock Twilio WhatsApp provider processing notification {NotificationId}.",
                notification.Id);

            if (Random.Shared.NextDouble() < 0.5)
            {
                _logger.LogWarning(
                    "Mock Twilio WhatsApp intentionally failed. NotificationId: {NotificationId}",
                    notification.Id);

                throw new InvalidOperationException(
                    "Mock Twilio WhatsApp provider failed intentionally.");
            }

            // Simulate external provider/network latency.
            await Task.Delay(
                TimeSpan.FromSeconds(5),
                cancellationToken);

            _logger.LogInformation(
                "Mock Twilio WhatsApp message sent successfully to {Recipient}. NotificationId: {NotificationId}",
                notification.Recipient,
                notification.Id);

            return new ProviderSendResult
            {
                IsSuccess = true,
                ProviderResponse =
                    "Mock Twilio WhatsApp message sent successfully."
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
                "Mock Twilio WhatsApp provider failed. NotificationId: {NotificationId}",
                notification.Id);

            return new ProviderSendResult
            {
                IsSuccess = false,
                ProviderResponse = "Failed to send an WhatsApp Message",
                ErrorMessage = ex.Message
            };
        }
    }
}