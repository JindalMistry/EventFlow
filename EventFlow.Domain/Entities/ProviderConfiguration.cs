using EventFlow.Domain.Common;
using EventFlow.Domain.Enums;

namespace EventFlow.Domain.Entities;

public class ProviderConfiguration : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public ProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Configuration { get; set; } = string.Empty;

    public Application Application { get; set; } = null!;
}
