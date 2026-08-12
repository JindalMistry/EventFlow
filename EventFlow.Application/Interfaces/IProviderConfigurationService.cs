using EventFlow.Application.Common;
using EventFlow.Application.DTOs.ProviderConfigurations;

namespace EventFlow.Application.Interfaces;

public interface IProviderConfigurationService
{
    Task<ProviderConfigurationResponse> CreateProviderConfigurationAsync(CreateProviderConfigurationRequest request);
    Task<PagedResponse<ProviderConfigurationResponse>> GetProviderConfigurationsAsync(ProviderConfigurationQueryParameters query);
    Task<ProviderConfigurationResponse> GetProviderConfigurationByIdAsync(Guid id);
    Task<ProviderConfigurationResponse> UpdateProviderConfigurationAsync(Guid id, UpdateProviderConfigurationRequest request);
    Task<ProviderConfigurationResponse> EnableProviderConfigurationAsync(Guid id);
    Task<ProviderConfigurationResponse> DisableProviderConfigurationAsync(Guid id);
    Task<TestProviderConnectionResponse> TestProviderConnectionAsync(Guid id);
}
