using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Domain.Enums;

namespace EventFlow.Application.Interfaces;

public interface IProviderTestService
{
    Task<TestProviderConnectionResponse> TestConnectionAsync(ProviderType providerType, string configurationJson);
}
