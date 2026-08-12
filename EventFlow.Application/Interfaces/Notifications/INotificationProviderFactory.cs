using EventFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Application.Interfaces.Notifications
{
    public interface INotificationProviderFactory
    {
        INotificationProvider GetProvider(NotificationChannel channel);
    }
}
