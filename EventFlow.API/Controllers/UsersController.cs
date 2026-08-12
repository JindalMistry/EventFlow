using EventFlow.Application.Common;
using EventFlow.Application.DTOs.Users;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = "RootAdmin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryParameters query)
    {
        var result = await _userService.GetUsersAsync(query);
        return Ok(new ApiResponse<PagedResponse<UserResponse>>
        {
            Success = true,
            Message = "Users retrieved successfully.",
            Data = result
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User retrieved successfully.",
            Data = result
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User created successfully.",
            Data = result
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User updated successfully.",
            Data = result
        });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        await _userService.UpdateUserStatusAsync(id, request);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User status updated successfully."
        });
    }

    [HttpPatch("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(id, request);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User password reset successfully."
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User deleted successfully."
        });
    }
}
