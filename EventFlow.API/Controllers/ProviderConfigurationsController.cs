using EventFlow.Application.Common;
using EventFlow.Application.DTOs.ProviderConfigurations;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("provider-configurations")]
[Authorize(Roles = "RootAdmin,ApplicationAdmin")]
public class ProviderConfigurationsController : ControllerBase
{
    private readonly IProviderConfigurationService _providerConfigurationService;

    public ProviderConfigurationsController(IProviderConfigurationService providerConfigurationService)
    {
        _providerConfigurationService = providerConfigurationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProviderConfiguration([FromBody] CreateProviderConfigurationRequest request)
    {
        var result = await _providerConfigurationService.CreateProviderConfigurationAsync(request);
        return CreatedAtAction(nameof(GetProviderConfigurationById), new { id = result.Id }, new ApiResponse<ProviderConfigurationResponse>
        {
            Success = true,
            Message = "Provider configuration created successfully.",
            Data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetProviderConfigurations([FromQuery] ProviderConfigurationQueryParameters query)
    {
        var result = await _providerConfigurationService.GetProviderConfigurationsAsync(query);
        return Ok(new ApiResponse<PagedResponse<ProviderConfigurationResponse>>
        {
            Success = true,
            Message = "Provider configurations retrieved successfully.",
            Data = result
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProviderConfigurationById(Guid id)
    {
        var result = await _providerConfigurationService.GetProviderConfigurationByIdAsync(id);
        return Ok(new ApiResponse<ProviderConfigurationResponse>
        {
            Success = true,
            Message = "Provider configuration details retrieved successfully.",
            Data = result
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProviderConfiguration(Guid id, [FromBody] UpdateProviderConfigurationRequest request)
    {
        var result = await _providerConfigurationService.UpdateProviderConfigurationAsync(id, request);
        return Ok(new ApiResponse<ProviderConfigurationResponse>
        {
            Success = true,
            Message = "Provider configuration updated successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/enable")]
    public async Task<IActionResult> EnableProviderConfiguration(Guid id)
    {
        var result = await _providerConfigurationService.EnableProviderConfigurationAsync(id);
        return Ok(new ApiResponse<ProviderConfigurationResponse>
        {
            Success = true,
            Message = "Provider configuration enabled successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/disable")]
    public async Task<IActionResult> DisableProviderConfiguration(Guid id)
    {
        var result = await _providerConfigurationService.DisableProviderConfigurationAsync(id);
        return Ok(new ApiResponse<ProviderConfigurationResponse>
        {
            Success = true,
            Message = "Provider configuration disabled successfully.",
            Data = result
        });
    }

    [HttpPost("{id:guid}/test")]
    public async Task<IActionResult> TestProviderConnection(Guid id)
    {
        var result = await _providerConfigurationService.TestProviderConnectionAsync(id);
        return Ok(new ApiResponse<TestProviderConnectionResponse>
        {
            Success = true,
            Message = "Provider connection test completed.",
            Data = result
        });
    }
}
