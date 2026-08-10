using System.ComponentModel.DataAnnotations;
using EventFlow.Domain.Enums;

namespace EventFlow.Application.DTOs.Users;

public class UpdateUserStatusRequest
{
    [Required]
    public UserStatus Status { get; set; }
}
