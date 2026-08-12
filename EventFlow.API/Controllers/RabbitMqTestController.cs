using EventFlow.Application.Interfaces.Messaging;
using EventFlow.Application.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("api/test/rabbitmq")]
public class RabbitMqTestController : ControllerBase
{
    private readonly IEventMessagePublisher _publisher;

    public RabbitMqTestController(IEventMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("publish")]
    public async Task<IActionResult> Publish(
        CancellationToken cancellationToken)
    {
        var message = new EventMessage(
            PublishedEventId: Guid.NewGuid(),
            ApplicationId: Guid.NewGuid(),
            EventDefinitionId: Guid.NewGuid(),
            CorrelationId: Guid.NewGuid());

        await _publisher.PublishAsync(message, cancellationToken);

        return Ok(new
        {
            Message = "Event message published successfully.",
            Event = message
        });
    }
}