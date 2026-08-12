using System.Text.Json;

namespace EventFlow.Application.DTOs.ProviderConfigurations;

public class ProviderConfigurationResponse
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public JsonElement Configuration { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
