namespace EventFlow.Application.DTOs.EventDefinitions;

public class UpdateEventDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
