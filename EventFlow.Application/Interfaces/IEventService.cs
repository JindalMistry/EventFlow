using EventFlow.Application.DTOs.Events;

namespace EventFlow.Application.Interfaces;

public interface IEventService
{
    Task<PublishEventResponse> PublishEventAsync(
        PublishEventRequest request,
        string? apiKey = null,
        string? appCode = null,
        CancellationToken cancellationToken = default);
}
