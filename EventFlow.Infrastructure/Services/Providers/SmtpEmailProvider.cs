using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.DTOs.TemplateEngine;
using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Providers;
using EventFlow.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text.Json;

namespace EventFlow.Infrastructure.Services.Providers
{
    public class SmtpEmailProvider : IEmailProvider
    {
        private readonly IProviderConfigurationValidator _configurationValidator;
        private readonly ILogger<SmtpEmailProvider> _logger;

        public SmtpEmailProvider(
        IProviderConfigurationValidator configurationValidator,
        ILogger<SmtpEmailProvider> logger)
        {
            _configurationValidator = configurationValidator;
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
                var smtpConfiguration =
                    configuration.Configuration.Deserialize<SmtpProviderConfigurationDto>(
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                    ?? throw new InvalidOperationException(
                        "Invalid SMTP provider configuration.");

                if (string.IsNullOrWhiteSpace(notification.Recipient))
                {
                    return new ProviderSendResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Notification recipient is required."
                    };
                }

                _logger.LogInformation("Email provider processing notification {NotificationId}.",
                notification.Id);
                //Temp Failure Mechanism

                if (Random.Shared.NextDouble() < 0.5)
                {
                    _logger.LogWarning(
                        "Email intentionally failed. NotificationId: {NotificationId}",
                        notification.Id);

                    throw new InvalidOperationException(
                        "Email provider failed intentionally.");
                }

                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(
                        smtpConfiguration.FromName,
                        smtpConfiguration.FromAddress));

                message.To.Add(
                    MailboxAddress.Parse(notification.Recipient));

                message.Subject = template.Subject ?? string.Empty;

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = template.Body
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();

                var secureSocketOptions =
                    smtpConfiguration.EnableSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None;

                await smtpClient.ConnectAsync(
                    smtpConfiguration.Host,
                    smtpConfiguration.Port,
                    secureSocketOptions,
                    cancellationToken);

                await smtpClient.AuthenticateAsync(
                    smtpConfiguration.UserName,
                    smtpConfiguration.Password,
                    cancellationToken);

                await smtpClient.SendAsync(
                    message,
                    cancellationToken);

                await smtpClient.DisconnectAsync(
                    true,
                    cancellationToken);

                _logger.LogInformation(
                    "Email sent successfully to {Recipient}. NotificationId: {NotificationId}",
                    notification.Recipient,
                    notification.Id);

                return new ProviderSendResult
                {
                    IsSuccess = true,
                    ProviderResponse = "Email sent successfully."
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
                    "Failed to send email. NotificationId: {NotificationId}",
                    notification.Id);

                return new ProviderSendResult
                {
                    IsSuccess = false,
                    ProviderResponse = "Failed to send an Email",
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
