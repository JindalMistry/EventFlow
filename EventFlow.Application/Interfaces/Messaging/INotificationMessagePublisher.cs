using EventFlow.Application.DTOs.Messaging;
using EventFlow.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Application.Interfaces.Messaging
{
    public interface INotificationMessagePublisher
    {
        Task PublishAsync(
            NotificationMessage message,
            CancellationToken cancellationToken = default);
    }
}
