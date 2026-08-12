using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Events;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PublishEvent(
        [FromBody] PublishEventRequest request,
        [FromHeader(Name = "X-API-Key")] string? apiKey = null,
        [FromHeader(Name = "X-Application-Code")] string? appCode = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _eventService.PublishEventAsync(request, apiKey, appCode, cancellationToken);

        _logger.LogInformation(
            "Event published successfully from {AppCode} application with {EventCode} event.",
            appCode,
            request.EventCode);

        return Ok(new ApiResponse<PublishEventResponse>
        {
            Success = true,
            Message = "Event published successfully.",
            Data = result
        });
    }
}
