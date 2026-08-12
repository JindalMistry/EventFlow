using EventFlow.Application.DTOs.NotificationTemplates;
using EventFlow.Application.DTOs.TemplateEngine;
using EventFlow.Domain.Entities;

namespace EventFlow.Application.Interfaces.Notifications
{
    public interface ITemplateEngine
    {
        RenderedTemplate Render(
            NotificationTemplateResponse template,
            string payload);
    }
}