using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.Applications;

public class ApplicationQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public ApplicationStatus? Status { get; set; }
}
