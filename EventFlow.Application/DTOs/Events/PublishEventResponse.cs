namespace EventFlow.Application.DTOs.Events;

public class PublishEventResponse
{
    public Guid PublishedEventId { get; set; }
    public Guid CorrelationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
