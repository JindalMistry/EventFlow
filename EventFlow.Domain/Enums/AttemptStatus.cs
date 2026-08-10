namespace EventFlow.Domain.Enums;

public enum AttemptStatus
{
    Success = 1,
    Failed = 2,
    Timeout = 3,
    Retrying = 4
}
