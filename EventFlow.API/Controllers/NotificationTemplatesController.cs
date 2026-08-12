using EventFlow.Application.Common;
using EventFlow.Application.DTOs.NotificationTemplates;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("notification-templates")]
[Authorize(Roles = "ApplicationAdmin")]
public class NotificationTemplatesController : ControllerBase
{
    private readonly INotificationTemplateService _notificationTemplateService;

    public NotificationTemplatesController(INotificationTemplateService notificationTemplateService)
    {
        _notificationTemplateService = notificationTemplateService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotificationTemplate([FromBody] CreateNotificationTemplateRequest request)
    {
        var result = await _notificationTemplateService.CreateNotificationTemplateAsync(request);
        return CreatedAtAction(nameof(GetNotificationTemplateById), new { id = result.Id }, new ApiResponse<NotificationTemplateResponse>
        {
            Success = true,
            Message = "Notification template created successfully.",
            Data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationTemplates([FromQuery] NotificationTemplateQueryParameters query)
    {
        var result = await _notificationTemplateService.GetNotificationTemplatesAsync(query);
        return Ok(new ApiResponse<PagedResponse<NotificationTemplateResponse>>
        {
            Success = true,
            Message = "Notification templates retrieved successfully.",
            Data = result
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNotificationTemplateById(Guid id)
    {
        var result = await _notificationTemplateService.GetNotificationTemplateByIdAsync(id);
        return Ok(new ApiResponse<NotificationTemplateResponse>
        {
            Success = true,
            Message = "Notification template details retrieved successfully.",
            Data = result
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateNotificationTemplate(Guid id, [FromBody] UpdateNotificationTemplateRequest request)
    {
        var result = await _notificationTemplateService.UpdateNotificationTemplateAsync(id, request);
        return Ok(new ApiResponse<NotificationTemplateResponse>
        {
            Success = true,
            Message = "Notification template updated successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateNotificationTemplate(Guid id)
    {
        var result = await _notificationTemplateService.ActivateNotificationTemplateAsync(id);
        return Ok(new ApiResponse<NotificationTemplateResponse>
        {
            Success = true,
            Message = "Notification template activated successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateNotificationTemplate(Guid id)
    {
        var result = await _notificationTemplateService.DeactivateNotificationTemplateAsync(id);
        return Ok(new ApiResponse<NotificationTemplateResponse>
        {
            Success = true,
            Message = "Notification template deactivated successfully.",
            Data = result
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNotificationTemplate(Guid id)
    {
        await _notificationTemplateService.DeleteNotificationTemplateAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Notification template deleted successfully."
        });
    }
}
