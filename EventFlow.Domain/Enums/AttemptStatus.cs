namespace EventFlow.Domain.Enums;

public enum AttemptStatus
{
    Processing = 1,
    Success = 2,
    Failed = 3,
    Timeout = 4,
    Retrying = 5
}
