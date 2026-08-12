namespace EventFlow.Application.DTOs.Applications;

public class RotateApiKeyResponse
{
    public Guid ApplicationId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public DateTime ApiKeyCreatedAt { get; set; }
}
