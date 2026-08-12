using System.Text.Json;

namespace EventFlow.Application.DTOs.LoadTest;

public sealed class LoadTestEvent
{
    public string ApplicationCode { get; set; } = string.Empty;

    public string EventCode { get; set; } = string.Empty;

    public JsonElement Payload { get; set; }
}