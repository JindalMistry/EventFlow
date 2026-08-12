using EventFlow.Application.Common;
using EventFlow.Application.DTOs.NotificationRules;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("notification-rules")]
[Authorize(Roles = "ApplicationAdmin")]
public class NotificationRulesController : ControllerBase
{
    private readonly INotificationRuleService _notificationRuleService;

    public NotificationRulesController(INotificationRuleService notificationRuleService)
    {
        _notificationRuleService = notificationRuleService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotificationRule([FromBody] CreateNotificationRuleRequest request)
    {
        var result = await _notificationRuleService.CreateNotificationRuleAsync(request);
        return CreatedAtAction(nameof(GetNotificationRuleById), new { id = result.Id }, new ApiResponse<NotificationRuleDetailResponse>
        {
            Success = true,
            Message = "Notification rule created successfully.",
            Data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationRules([FromQuery] NotificationRuleQueryParameters query)
    {
        var result = await _notificationRuleService.GetNotificationRulesAsync(query);
        return Ok(new ApiResponse<PagedResponse<NotificationRuleResponse>>
        {
            Success = true,
            Message = "Notification rules retrieved successfully.",
            Data = result
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNotificationRuleById(Guid id)
    {
        var result = await _notificationRuleService.GetNotificationRuleByIdAsync(id);
        return Ok(new ApiResponse<NotificationRuleDetailResponse>
        {
            Success = true,
            Message = "Notification rule details retrieved successfully.",
            Data = result
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateNotificationRule(Guid id, [FromBody] UpdateNotificationRuleRequest request)
    {
        var result = await _notificationRuleService.UpdateNotificationRuleAsync(id, request);
        return Ok(new ApiResponse<NotificationRuleDetailResponse>
        {
            Success = true,
            Message = "Notification rule updated successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/enable")]
    public async Task<IActionResult> EnableNotificationRule(Guid id)
    {
        var result = await _notificationRuleService.EnableNotificationRuleAsync(id);
        return Ok(new ApiResponse<NotificationRuleDetailResponse>
        {
            Success = true,
            Message = "Notification rule enabled successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/disable")]
    public async Task<IActionResult> DisableNotificationRule(Guid id)
    {
        var result = await _notificationRuleService.DisableNotificationRuleAsync(id);
        return Ok(new ApiResponse<NotificationRuleDetailResponse>
        {
            Success = true,
            Message = "Notification rule disabled successfully.",
            Data = result
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNotificationRule(Guid id)
    {
        await _notificationRuleService.DeleteNotificationRuleAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Notification rule deleted successfully."
        });
    }
}
