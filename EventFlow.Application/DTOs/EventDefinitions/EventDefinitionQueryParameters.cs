namespace EventFlow.Application.DTOs.EventDefinitions;

public class EventDefinitionQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public Guid? ApplicationId { get; set; }
    public bool? IsActive { get; set; }
}
