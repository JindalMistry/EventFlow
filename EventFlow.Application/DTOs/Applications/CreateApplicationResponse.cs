namespace EventFlow.Application.DTOs.Applications;

public class CreateApplicationResponse : ApplicationResponse
{
    public string ApiKey { get; set; } = string.Empty;
}
