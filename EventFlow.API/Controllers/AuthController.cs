using System.Security.Claims;
using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Auth;
using EventFlow.Application.DTOs.Users;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful.",
            Data = result
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserIdFromClaims();
        await _authService.ChangePasswordAsync(userId, request);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Password changed successfully."
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromClaims();
        var result = await _authService.GetMeAsync(userId);
        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "Profile retrieved successfully.",
            Data = result
        });
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid user identity claim.");
        }

        return userId;
    }
}
