using System.Text.Json;
using EventFlow.Domain.Enums;

namespace EventFlow.Application.Interfaces;

public interface IProviderConfigurationValidator
{
    Task<object> ValidateAndDeserializeAsync(
        ProviderType providerType,
        JsonElement configuration,
        CancellationToken cancellationToken = default);

    object Deserialize(
        ProviderType providerType,
        string configurationJson);

    T Deserialize<T>(
        ProviderType providerType,
        string configurationJson);
}
