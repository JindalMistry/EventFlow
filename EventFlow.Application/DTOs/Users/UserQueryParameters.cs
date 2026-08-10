using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.Users;

public class UserQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public Guid? ApplicationId { get; set; }
    public UserStatus? Status { get; set; }
}
