namespace EventFlow.Application.DTOs.EventDefinitions;

public class CreateEventDefinitionRequest
{
    public Guid ApplicationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
