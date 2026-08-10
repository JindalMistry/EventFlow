namespace EventFlow.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
    Guid? ApplicationId { get; }
    bool IsRootAdmin { get; }
    bool IsApplicationAdmin { get; }
}
