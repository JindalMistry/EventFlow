using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.ProviderConfigurations;

public class ProviderConfigurationQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public ProviderType? ProviderType { get; set; }
    public bool? IsEnabled { get; set; }
    public Guid? ApplicationId { get; set; }
}
