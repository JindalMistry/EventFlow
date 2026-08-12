using EventFlow.Application.Common;
using EventFlow.Application.DTOs.EventDefinitions;

namespace EventFlow.Application.Interfaces;

public interface IEventDefinitionService
{
    Task<EventDefinitionResponse> CreateEventDefinitionAsync(CreateEventDefinitionRequest request);
    Task<PagedResponse<EventDefinitionResponse>> GetEventDefinitionsAsync(EventDefinitionQueryParameters query);
    Task<EventDefinitionResponse> GetEventDefinitionByIdAsync(Guid id);
    Task<EventDefinitionResponse> UpdateEventDefinitionAsync(Guid id, UpdateEventDefinitionRequest request);
    Task<EventDefinitionResponse> ActivateEventDefinitionAsync(Guid id);
    Task<EventDefinitionResponse> DeactivateEventDefinitionAsync(Guid id);
    Task DeleteEventDefinitionAsync(Guid id);
}
