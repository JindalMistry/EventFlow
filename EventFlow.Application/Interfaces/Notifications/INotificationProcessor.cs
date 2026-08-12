using EventFlow.Application.DTOs.Messaging;
using EventFlow.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Application.Interfaces.Notifications
{
    public interface INotificationProcessor
    {
        Task ProcessAsync(
            NotificationMessage message, 
            CancellationToken cancellationToken = default);
    }
}
