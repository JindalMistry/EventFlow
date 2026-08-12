using EventFlow.Application.DTOs.Messaging;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Messaging;
using EventFlow.Application.Interfaces.Notifications;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Infrastructure.Services
{
    public class NotificationProcessor : INotificationProcessor
    {
        private readonly INotificationService _notificationService;
        private readonly IPublishedEventService _publishedEventService;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly INotificationAttemptService _notificationAttemptService;
        private readonly ITemplateEngine _templateEngine;
        private readonly INotificationProviderFactory _providerFactory;
        private readonly INotificationDeadLetterMessagePublisher _notificationDeadLetterMessagePublisher;
        private readonly IRetryMessagePublisher _retryMessagePublisher;

        public NotificationProcessor(
            INotificationService notificationService,
            IPublishedEventService publishedEventService,
            INotificationTemplateService notificationTemplateService,
            IProviderConfigurationService providerConfigurationService,
            INotificationAttemptService notificationAttemptService,
            ITemplateEngine templateEngine,
            INotificationProviderFactory providerFactory,
            INotificationDeadLetterMessagePublisher notificationDeadLetterMessagePublisher,
            IRetryMessagePublisher retryMessagePublisher)
        {
            _notificationService = notificationService;
            _publishedEventService = publishedEventService;
            _notificationTemplateService = notificationTemplateService;
            _providerConfigurationService = providerConfigurationService;
            _notificationAttemptService = notificationAttemptService;
            _templateEngine = templateEngine;
            _providerFactory = providerFactory;
            _notificationDeadLetterMessagePublisher = notificationDeadLetterMessagePublisher;
            _retryMessagePublisher = retryMessagePublisher;
        }

        public async Task ProcessAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            var notification = await _notificationService.GetByIdAsync(
                    message.NotificationId,
                    cancellationToken);

            if (notification is null)
            {
                throw new NotFoundException(
                    $"Notification '{message.NotificationId}' was not found.");
            }

            var publishedEvent = await _publishedEventService.GetByIdAsync(
                    notification.EventId,
                    cancellationToken);

            if (publishedEvent is null)
            {
                throw new NotFoundException(
                    $"Published event '{notification.EventId}' was not found.");
            }

            //TODO : ADD Cancellation token
            var template = await _notificationTemplateService.GetTemplateById(notification.NotificationTemplateId, cancellationToken);

            if (template is null)
            {
                throw new NotFoundException(
                    $"Notification template '{notification.NotificationTemplateId}' was not found.");
            }

            //TODO : ADD Cancellation token
            var providerConfiguration = await _providerConfigurationService.GetProviderConfigurationByIdAsync(
                notification.ProviderConfigurationId);

            if (providerConfiguration is null)
            {
                throw new NotFoundException(
                    $"Provider configuration '{notification.ProviderConfigurationId}' was not found.");
            }

            var latestAttempt = await _notificationAttemptService.GetLatestAttempByNotificationId(
                notification.Id, 
                cancellationToken);

            var attemptNumber = latestAttempt is null
                ? 1
                : latestAttempt.AttemptNumber + 1;

            if (attemptNumber > notification.NotificationRule.RetryCount)
            {
                var dlMessage = new NotificationMessage
                {
                    NotificationId = notification.Id,
                };

                await _notificationDeadLetterMessagePublisher.PublishAsync(
                    dlMessage, 
                    cancellationToken);

                return;
            }

            var attempt = new NotificationAttempt
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                AttemptNumber = attemptNumber,
                Status = AttemptStatus.Processing,
                StartedAt = DateTime.UtcNow
            };

            await _notificationAttemptService.CreateAsync(
                attempt,
                cancellationToken);

            await _notificationService.UpdateStatusAsync(
                notification.Id,
                NotificationStatus.Processing,
                cancellationToken);

            var renderedTemplate = _templateEngine.Render(template, publishedEvent.Payload);

            try
            {
                INotificationProvider provider = _providerFactory.GetProvider(notification.Channel);
                var result = await provider.SendAsync(
                    notification,
                    renderedTemplate,
                    providerConfiguration,
                    cancellationToken);

                if (!result.IsSuccess)
                {
                    await HandleFailureAsync(
                        notification,
                        attempt,
                        result.ErrorMessage,
                        result.ProviderResponse,
                        cancellationToken,
                        notification.NotificationRule.RetryCount);

                    return;
                }

                var completedAt = DateTime.UtcNow;

                await _notificationAttemptService.CompleteAsync(
                    attempt.Id,
                    AttemptStatus.Success,
                    null,
                    result.ProviderResponse,
                    completedAt,
                    cancellationToken);

                await _notificationService.CompleteProcessingAsync(
                    notification.Id,
                    NotificationStatus.Succeeded,
                    null,
                    completedAt,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await HandleFailureAsync(
                    notification,
                    attempt,
                    ex.Message,
                    null,
                    cancellationToken,
                    notification.NotificationRule.RetryCount);
            }
        }

        private async Task HandleFailureAsync(
            Notification notification,
            NotificationAttempt attempt,
            string? errorMessage,
            string? providerResponse,
            CancellationToken cancellationToken,
            int RetryCount)
        {
            var message = new NotificationMessage
            {
                NotificationId = notification.Id,
            };

            var failedAt = DateTime.UtcNow;

            await _notificationAttemptService.CompleteAsync(
                attempt.Id,
                AttemptStatus.Failed,
                errorMessage,
                providerResponse,
                failedAt,
                cancellationToken);

            if (attempt.AttemptNumber < RetryCount)
            {
                await _notificationService.UpdateStatusAsync(
                    notification.Id,
                    NotificationStatus.Retrying,
                    cancellationToken);


                await _retryMessagePublisher.PublishAsync(
                    message,
                    cancellationToken);

                return;
            }

            await _notificationService.CompleteProcessingAsync(
                notification.Id,
                NotificationStatus.Failed,
                errorMessage,
                failedAt,
                cancellationToken);

            await _notificationDeadLetterMessagePublisher.PublishAsync(message, cancellationToken);
        }
    }
}
