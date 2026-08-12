namespace EventFlow.Application.DTOs.Applications;

public class ApplicationDetailResponse : ApplicationResponse
{
    public int TotalApplicationAdminUsers { get; set; }
    public int TotalProviderConfigurations { get; set; }
    public int TotalEventDefinitions { get; set; }
    public int TotalNotificationRules { get; set; }
    public int TotalNotificationTemplates { get; set; }
}
