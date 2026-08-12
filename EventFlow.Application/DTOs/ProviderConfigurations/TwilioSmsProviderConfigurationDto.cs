namespace EventFlow.Application.DTOs.ProviderConfigurations;

public class TwilioSmsProviderConfigurationDto
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
}
