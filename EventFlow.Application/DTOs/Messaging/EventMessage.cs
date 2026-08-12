namespace EventFlow.Application.Messaging;

public sealed record EventMessage(
    Guid PublishedEventId,
    Guid ApplicationId,
    Guid EventDefinitionId,
    Guid CorrelationId
);