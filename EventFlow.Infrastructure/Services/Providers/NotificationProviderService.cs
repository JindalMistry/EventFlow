using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces.Notifications;
using EventFlow.Application.Interfaces.Providers;
using EventFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Infrastructure.Services.Providers
{
    public class NotificationProviderFactory : INotificationProviderFactory
    {
        private readonly IEmailProvider _emailProvider;
        private readonly ISmsProvider _smsProvider;
        private readonly IWhatsAppProvider _whatsAppProvider;

        public NotificationProviderFactory(
            IEmailProvider emailProvider,
            ISmsProvider smsProvider,
            IWhatsAppProvider whatsAppProvider)

        {
            _emailProvider = emailProvider;
            _smsProvider = smsProvider;
            _whatsAppProvider = whatsAppProvider;
        }

        public INotificationProvider GetProvider(
            NotificationChannel channel)
        {
            return channel switch
            {
                NotificationChannel.Email => _emailProvider,
                NotificationChannel.Sms => _smsProvider,
                NotificationChannel.WhatsApp => _whatsAppProvider,

                _ => throw new BadRequestException(
                    $"Unsupported notification channel: {channel}")
            };
        }
    }
}
