namespace EventFlow.Application.DTOs.Applications;

public class ApplicationResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SupportEmail { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasApiKey { get; set; }
    public DateTime ApiKeyCreatedAt { get; set; }
    public DateTime? ApiKeyExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
