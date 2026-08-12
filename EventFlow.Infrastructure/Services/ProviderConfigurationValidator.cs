using System.Text.Json;
using System.Text.Json.Serialization;
using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Domain.Enums;

namespace EventFlow.Infrastructure.Services;

public class ProviderConfigurationValidator : IProviderConfigurationValidator
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public Task<object> ValidateAndDeserializeAsync(
        ProviderType providerType,
        JsonElement configuration,
        CancellationToken cancellationToken = default)
    {
        var validatedObject = ValidateAndDeserialize(providerType, configuration);
        return Task.FromResult(validatedObject);
    }

    public object Deserialize(ProviderType providerType, string configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            throw new BadRequestException("Configuration is required.");
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(configurationJson);
        }
        catch
        {
            throw new BadRequestException("Configuration must be a valid JSON object.");
        }

        using (doc)
        {
            return ValidateAndDeserialize(providerType, doc.RootElement);
        }
    }

    public T Deserialize<T>(ProviderType providerType, string configurationJson)
    {
        var result = Deserialize(providerType, configurationJson);
        if (result is T typedResult)
        {
            return typedResult;
        }

        throw new BadRequestException($"Configuration for provider type '{providerType}' cannot be converted to {typeof(T).Name}.");
    }

    private static object ValidateAndDeserialize(ProviderType providerType, JsonElement configuration)
    {
        if (!Enum.IsDefined(typeof(ProviderType), providerType))
        {
            throw new BadRequestException("Invalid provider type.");
        }

        if (configuration.ValueKind != JsonValueKind.Object)
        {
            throw new BadRequestException("Configuration must be a valid JSON object.");
        }

        var json = configuration.GetRawText();

        switch (providerType)
        {
            case ProviderType.Smtp:
                return ValidateSmtp(json);

            case ProviderType.TwilioSms:
                return ValidateTwilioSms(json);

            case ProviderType.TwilioWhatsApp:
                return ValidateTwilioWhatsApp(json);

            default:
                throw new BadRequestException("Invalid provider type.");
        }
    }

    private static SmtpProviderConfigurationDto ValidateSmtp(string json)
    {
        SmtpProviderConfigurationDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<SmtpProviderConfigurationDto>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException)
        {
            throw new BadRequestException("Invalid SMTP provider configuration.");
        }

        if (dto == null)
        {
            throw new BadRequestException("Invalid SMTP provider configuration.");
        }

        if (string.IsNullOrWhiteSpace(dto.Host))
        {
            throw new BadRequestException("SMTP Host is required.");
        }

        if (dto.Port <= 0 || dto.Port > 65535)
        {
            throw new BadRequestException("SMTP Port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(dto.UserName))
        {
            throw new BadRequestException("SMTP UserName is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new BadRequestException("SMTP Password is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.FromAddress))
        {
            throw new BadRequestException("SMTP FromAddress is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.FromName))
        {
            throw new BadRequestException("SMTP FromName is required.");
        }

        return dto;
    }

    private static TwilioSmsProviderConfigurationDto ValidateTwilioSms(string json)
    {
        TwilioSmsProviderConfigurationDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<TwilioSmsProviderConfigurationDto>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException)
        {
            throw new BadRequestException("Invalid Twilio SMS provider configuration.");
        }

        if (dto == null)
        {
            throw new BadRequestException("Invalid Twilio SMS provider configuration.");
        }

        if (string.IsNullOrWhiteSpace(dto.AccountSid))
        {
            throw new BadRequestException("Twilio AccountSid is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.AuthToken))
        {
            throw new BadRequestException("Twilio AuthToken is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.FromPhoneNumber))
        {
            throw new BadRequestException("Twilio FromPhoneNumber is required.");
        }

        return dto;
    }

    private static TwilioWhatsAppProviderConfigurationDto ValidateTwilioWhatsApp(string json)
    {
        TwilioWhatsAppProviderConfigurationDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<TwilioWhatsAppProviderConfigurationDto>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException)
        {
            throw new BadRequestException("Invalid Twilio WhatsApp provider configuration.");
        }

        if (dto == null)
        {
            throw new BadRequestException("Invalid Twilio WhatsApp provider configuration.");
        }

        if (string.IsNullOrWhiteSpace(dto.AccountSid))
        {
            throw new BadRequestException("Twilio AccountSid is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.AuthToken))
        {
            throw new BadRequestException("Twilio AuthToken is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.FromPhoneNumber))
        {
            throw new BadRequestException("Twilio FromPhoneNumber is required.");
        }

        return dto;
    }
}
