namespace EventFlow.Application.DTOs.Events;

public class PublishEventRequest
{
    public string EventCode { get; set; } = string.Empty;
    public object Payload { get; set; } = null!;
}
