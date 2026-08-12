using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Applications;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("applications")]
[Authorize(Roles = "RootAdmin")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
    {
        var result = await _applicationService.CreateApplicationAsync(request);
        return CreatedAtAction(nameof(GetApplicationById), new { id = result.Id }, new ApiResponse<CreateApplicationResponse>
        {
            Success = true,
            Message = "Application created successfully.",
            Data = result
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] UpdateApplicationRequest request)
    {
        var result = await _applicationService.UpdateApplicationAsync(id, request);
        return Ok(new ApiResponse<ApplicationResponse>
        {
            Success = true,
            Message = "Application updated successfully.",
            Data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] ApplicationQueryParameters query)
    {
        var result = await _applicationService.GetApplicationsAsync(query);
        return Ok(new ApiResponse<PagedResponse<ApplicationResponse>>
        {
            Success = true,
            Message = "Applications retrieved successfully.",
            Data = result
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetApplicationById(Guid id)
    {
        var result = await _applicationService.GetApplicationByIdAsync(id);
        return Ok(new ApiResponse<ApplicationDetailResponse>
        {
            Success = true,
            Message = "Application details retrieved successfully.",
            Data = result
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteApplication(Guid id)
    {
        await _applicationService.DeleteApplicationAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Application deleted successfully."
        });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateApplication(Guid id)
    {
        await _applicationService.ActivateApplicationAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Application activated successfully."
        });
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateApplication(Guid id)
    {
        await _applicationService.DeactivateApplicationAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Application deactivated successfully."
        });
    }

    [HttpPost("{id:guid}/rotate-api-key")]
    public async Task<IActionResult> RotateApiKey(Guid id)
    {
        var result = await _applicationService.RotateApiKeyAsync(id);
        return Ok(new ApiResponse<RotateApiKeyResponse>
        {
            Success = true,
            Message = "API key rotated successfully.",
            Data = result
        });
    }
}
