namespace EventFlow.Application.DTOs.LoadTest;

public sealed class LoadTestResult
{
    public bool Success { get; set; }

    public string ApplicationCode { get; set; } = string.Empty;

    public string EventCode { get; set; } = string.Empty;

    public int? StatusCode { get; set; }

    public string? Error { get; set; }
}