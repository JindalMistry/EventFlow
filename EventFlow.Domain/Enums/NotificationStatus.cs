namespace EventFlow.Domain.Enums;

public enum NotificationStatus
{
    Pending = 1,
    Queued = 2,
    Processing = 3,
    Succeeded = 4,
    Failed = 5,
    Retrying = 6,
    DeadLettered = 7
}
