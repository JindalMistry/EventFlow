using System.ComponentModel.DataAnnotations;

namespace EventFlow.Application.DTOs.Applications;

public class UpdateApplicationRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    public string? SupportEmail { get; set; }
}
