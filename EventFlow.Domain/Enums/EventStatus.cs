namespace EventFlow.Domain.Enums;

public enum EventStatus
{
    Received = 1,
    Queued = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5
}
