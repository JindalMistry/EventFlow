using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.DTOs.TemplateEngine;
using EventFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Application.Interfaces.Notifications
{
    public interface INotificationProvider
    {
        Task<ProviderSendResult> SendAsync(
            Notification notification,
            RenderedTemplate template,
            ProviderConfigurationResponse configuration,
            CancellationToken cancellationToken = default);
    }
}
