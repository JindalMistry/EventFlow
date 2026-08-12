using EventFlow.Application.Common;
using EventFlow.Application.DTOs.EventDefinitions;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("event-definitions")]
[Authorize(Roles = "RootAdmin,ApplicationAdmin")]
public class EventDefinitionsController : ControllerBase
{
    private readonly IEventDefinitionService _eventDefinitionService;

    public EventDefinitionsController(IEventDefinitionService eventDefinitionService)
    {
        _eventDefinitionService = eventDefinitionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEventDefinition([FromBody] CreateEventDefinitionRequest request)
    {
        var result = await _eventDefinitionService.CreateEventDefinitionAsync(request);
        return CreatedAtAction(nameof(GetEventDefinitionById), new { id = result.Id }, new ApiResponse<EventDefinitionResponse>
        {
            Success = true,
            Message = "Event definition created successfully.",
            Data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetEventDefinitions([FromQuery] EventDefinitionQueryParameters query)
    {
        var result = await _eventDefinitionService.GetEventDefinitionsAsync(query);
        return Ok(new ApiResponse<PagedResponse<EventDefinitionResponse>>
        {
            Success = true,
            Message = "Event definitions retrieved successfully.",
            Data = result
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEventDefinitionById(Guid id)
    {
        var result = await _eventDefinitionService.GetEventDefinitionByIdAsync(id);
        return Ok(new ApiResponse<EventDefinitionResponse>
        {
            Success = true,
            Message = "Event definition details retrieved successfully.",
            Data = result
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEventDefinition(Guid id, [FromBody] UpdateEventDefinitionRequest request)
    {
        var result = await _eventDefinitionService.UpdateEventDefinitionAsync(id, request);
        return Ok(new ApiResponse<EventDefinitionResponse>
        {
            Success = true,
            Message = "Event definition updated successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateEventDefinition(Guid id)
    {
        var result = await _eventDefinitionService.ActivateEventDefinitionAsync(id);
        return Ok(new ApiResponse<EventDefinitionResponse>
        {
            Success = true,
            Message = "Event definition activated successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateEventDefinition(Guid id)
    {
        var result = await _eventDefinitionService.DeactivateEventDefinitionAsync(id);
        return Ok(new ApiResponse<EventDefinitionResponse>
        {
            Success = true,
            Message = "Event definition deactivated successfully.",
            Data = result
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEventDefinition(Guid id)
    {
        await _eventDefinitionService.DeleteEventDefinitionAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Event definition deleted successfully."
        });
    }
}
