using System.ComponentModel.DataAnnotations;

namespace SmartCampus.DTOs;

public class CreateServiceRequestDto
{
    [Required]
    public string RequestType { get; set; } = string.Empty;

    [Required, StringLength(140, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
}

public class UpdateServiceRequestDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AdminRemarks { get; set; }
}

public class ServiceRequestResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

