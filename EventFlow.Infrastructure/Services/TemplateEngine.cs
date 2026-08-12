using EventFlow.Application.DTOs.NotificationTemplates;
using EventFlow.Application.DTOs.TemplateEngine;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces.Notifications;
using EventFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EventFlow.Infrastructure.Services
{
    public class TemplateEngine : ITemplateEngine
    {
        private static readonly Regex PlaceholderRegex =
        new(@"\{\{(?<property>[a-zA-Z0-9_.]+)\}\}",
            RegexOptions.Compiled);

        public RenderedTemplate Render(NotificationTemplateResponse template, string payload)
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            var subject = ReplacePlaceholders(
            template.Subject,
            root);

            var body = ReplacePlaceholders(
                template.Body,
                root);

            return new RenderedTemplate
            {
                Subject = subject,
                Body = body ?? ""
            };
        }

        private static string? ReplacePlaceholders(
            string? template,
            JsonElement payload)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }

            return PlaceholderRegex.Replace(
                template,
                match =>
                {
                    var propertyName =
                        match.Groups["property"].Value;

                    if (!payload.TryGetProperty(
                            propertyName,
                            out var value))
                    {
                        throw new NotFoundException(
                            $"Template property '{propertyName}' was not found in the event payload.");
                    }

                    return value.ValueKind switch
                    {
                        JsonValueKind.String =>
                            value.GetString() ?? string.Empty,

                        JsonValueKind.Number =>
                            value.ToString(),

                        JsonValueKind.True =>
                            "true",

                        JsonValueKind.False =>
                            "false",

                        JsonValueKind.Null =>
                            string.Empty,

                        _ =>
                            value.ToString()
                    };
                });
        }
    }
}
