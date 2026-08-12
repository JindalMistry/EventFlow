using System.Text.Json;
using EventFlow.Application.DTOs.Events;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using EventFlow.Application.Interfaces.Messaging;
using EventFlow.Application.Messaging;
using EventFlow.Domain.Enums;
using EventFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly EventFlowDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPublishedEventService _publishedEventService;
    private readonly IEventMessagePublisher _eventMessagePublisher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EventService(
        EventFlowDbContext dbContext,
        IPasswordHasher passwordHasher,
        IPublishedEventService publishedEventService,
        IEventMessagePublisher eventMessagePublisher,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _publishedEventService = publishedEventService;
        _eventMessagePublisher = eventMessagePublisher;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PublishEventResponse> PublishEventAsync(
        PublishEventRequest request,
        string? apiKey = null,
        string? appCode = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new BadRequestException("Event request cannot be null.");
        }

        // 1. Resolve & Validate Headers
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new UnauthorizedException("API key is required.");
        }

        if (string.IsNullOrWhiteSpace(appCode))
        {
            throw new UnauthorizedException("Application code is required.");
        }

        // 2. Direct Application Lookup by Code
        var trimmedAppCode = appCode.Trim();

        var application = await _dbContext.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Code.ToUpper() == trimmedAppCode.ToUpper() && !a.IsDeleted, cancellationToken);

        if (application == null || !_passwordHasher.Verify(apiKey, application.ApiKeyHash))
        {
            throw new UnauthorizedException("Invalid API key or application code.");
        }

        if (application.Status != ApplicationStatus.Active)
        {
            throw new UnauthorizedException($"Application '{application.Name}' is not active.");
        }

        if (application.ApiKeyExpiresAt.HasValue && application.ApiKeyExpiresAt.Value < DateTime.UtcNow)
        {
            throw new UnauthorizedException("API key has expired.");
        }

        // 3. Direct EventDefinition Lookup by ApplicationId + EventCode
        if (string.IsNullOrWhiteSpace(request.EventCode))
        {
            throw new BadRequestException("Event code is required.");
        }

        var trimmedEventCode = request.EventCode.Trim();

        var eventDefinition = await _dbContext.EventDefinitions
            .AsNoTracking()
            .Where(o => o.ApplicationId == application.Id && o.IsDeleted == false && o.Code == trimmedEventCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (eventDefinition == null)
        {
            throw new NotFoundException($"Event definition '{trimmedEventCode}' was not found for application '{application.Code}'.");
        }

        if (!eventDefinition.IsActive)
        {
            throw new BadRequestException($"Event definition '{trimmedEventCode}' is inactive.");
        }

        // 4. Serialize Dynamic Payload & Create PublishedEvent
        if (request.Payload == null)
        {
            throw new BadRequestException("Event payload is required.");
        }

        string serializedPayload = request.Payload switch
        {
            JsonElement jsonElement => jsonElement.GetRawText(),
            string rawStr => rawStr,
            _ => JsonSerializer.Serialize(request.Payload)
        };

        if (string.IsNullOrWhiteSpace(serializedPayload))
        {
            throw new BadRequestException("Event payload cannot be empty.");
        }

        var correlationId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var publishedEvent = await _publishedEventService.CreateAsync(
            applicationId: application.Id,
            eventDefinitionId: eventDefinition.Id,
            correlationId: correlationId,
            payload: serializedPayload,
            occurredAt: occurredAt,
            status: EventStatus.Received,
            cancellationToken: cancellationToken);

        // 5. Publish EventMessage to RabbitMQ
        var eventMessage = new EventMessage(
            PublishedEventId: publishedEvent.Id,
            ApplicationId: application.Id,
            EventDefinitionId: eventDefinition.Id,
            CorrelationId: correlationId);

        await _eventMessagePublisher.PublishAsync(eventMessage, cancellationToken);

        await _publishedEventService.UpdatePublishedEventStatusAsync(publishedEvent.Id, EventStatus.Queued, cancellationToken);

        // 6. Return Response
        return new PublishEventResponse
        {
            PublishedEventId = publishedEvent.Id,
            CorrelationId = correlationId,
            Status = publishedEvent.Status.ToString(),
            OccurredAt = publishedEvent.OccurredAt
        };
    }
}
