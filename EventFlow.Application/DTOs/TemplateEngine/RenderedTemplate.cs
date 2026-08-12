namespace EventFlow.Application.DTOs.TemplateEngine
{
    public sealed class RenderedTemplate
    {
        public string? Subject { get; init; }
        public string Body { get; init; } = string.Empty;
    }
}