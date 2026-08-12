using EventFlow.Application.DTOs.Messaging;
using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Events;
using EventFlow.Application.Interfaces.Messaging;
using EventFlow.Application.Interfaces.Notifications;
using EventFlow.Application.Messaging;
using EventFlow.Domain.Entities;
using EventFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;

namespace EventFlow.Infrastructure.Services
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IPublishedEventService _publishedEventService;
        private readonly INotificationRuleService _notificationRuleService;
        private readonly INotificationService _notificationService;
        private readonly INotificationMessagePublisher _notificationMessagePublisher;

        public EventProcessor(
        IPublishedEventService publishedEventService,
        INotificationRuleService notificationRuleService,
        INotificationService notificationService,
        INotificationMessagePublisher notificationMessagePublisher)
        {
            _publishedEventService = publishedEventService;
            _notificationRuleService = notificationRuleService;
            _notificationService = notificationService;
            _notificationMessagePublisher = notificationMessagePublisher;
        }
        public async Task ProcessAsync(EventMessage message, CancellationToken cancellationToken = default)
        {
            var publishedEvent =
            await _publishedEventService.GetByIdAsync(
                message.PublishedEventId,
                cancellationToken);

            if (publishedEvent is null)
            {
                throw new InvalidOperationException(
                    $"Published event '{message.PublishedEventId}' was not found.");
            }

            await _publishedEventService.UpdatePublishedEventStatusAsync(
                publishedEvent.Id,
                EventStatus.Processing,
                cancellationToken);

            var notificationRules =
            await _notificationRuleService.GetNotificationRulesByEventIdAsync(publishedEvent.EventDefinitionId, cancellationToken);

            foreach (var rule in notificationRules)
            {
                var existingNotification = await _notificationService.GetByEventAndRuleAsync(
                                                publishedEvent.Id,
                                                rule.Id, 
                                                cancellationToken);

                if (existingNotification is not null)
                {
                    continue;
                }

                var channel = Enum.Parse<NotificationChannel>(
                        rule.Channel,
                        ignoreCase: true);

                var recipient = ResolveRecipient(
                        channel,
                        publishedEvent.Payload);

                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    EventId = publishedEvent.Id,
                    NotificationRuleId = rule.Id,
                    NotificationTemplateId = rule.NotificationTemplateId,
                    ProviderConfigurationId = rule.ProviderConfigurationId,
                    Channel = channel,
                    Status = NotificationStatus.Queued,
                    QueuedAt = DateTime.UtcNow,
                    Recipient = recipient
                };

                await _notificationService.CreateAsync(
                    notification,
                    cancellationToken);

                var notificationMessage = new NotificationMessage
                {
                    NotificationId = notification.Id
                };

                await _notificationMessagePublisher.PublishAsync(
                    notificationMessage,
                    cancellationToken);
            }

            await _publishedEventService.CompleteProcessingAsync(
                publishedEvent.Id,
                EventStatus.Completed,
                DateTime.UtcNow,
                cancellationToken);
        }

        private string ResolveRecipient(
            NotificationChannel channel,
            string payload)
        {
            using var document = JsonDocument.Parse(payload);

            var json = document.RootElement;

            return channel switch
            {
                NotificationChannel.Email =>
                    json.GetProperty("email").GetString()
                    ?? throw new InvalidOperationException("Email is required."),

                NotificationChannel.Sms or NotificationChannel.WhatsApp =>
                    json.GetProperty("phone").GetString()
                    ?? throw new InvalidOperationException("Phone is required."),

                _ => throw new InvalidOperationException(
                    $"Unsupported notification channel: {channel}")
            };
        }
    }
}
