using System.Text.Json;
using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.ProviderConfigurations;

public class UpdateProviderConfigurationRequest
{
    public ProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public JsonElement Configuration { get; set; }
    public bool IsEnabled { get; set; }
}
