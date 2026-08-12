using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Enums;

namespace EventFlow.Infrastructure.Services;

public class ProviderTestService : IProviderTestService
{
    public Task<TestProviderConnectionResponse> TestConnectionAsync(ProviderType providerType, string configurationJson)
    {
        // Currently, actual external SDK integrations (Twilio / WhatsApp / SMTP live connectivity) are not implemented.
        // We return a clear response indicating that connection testing is not implemented for this provider, without faking success.
        var response = new TestProviderConnectionResponse
        {
            Success = false,
            Message = "Provider connection testing is not implemented for this provider."
        };

        return Task.FromResult(response);
    }
}
