using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Application.DTOs.ProviderConfigurations
{
    public sealed class ProviderSendResult
    {
        public bool IsSuccess { get; init; }
        public string? ProviderResponse { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
