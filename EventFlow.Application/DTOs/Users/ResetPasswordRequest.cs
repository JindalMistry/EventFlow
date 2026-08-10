using System.ComponentModel.DataAnnotations;

namespace EventFlow.Application.DTOs.Users;

public class ResetPasswordRequest
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
