using System.ComponentModel.DataAnnotations;

namespace SmartCampus.DTOs;

public class CreateComplaintDto
{
    [Required, StringLength(140, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Category { get; set; } = "General";
    [Required]
    public string DepartmentId { get; set; } = string.Empty;
    [Required, StringLength(3000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class UpdateComplaintDto
{
    [Required, StringLength(140, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Category { get; set; } = string.Empty;
    [Required]
    public string DepartmentId { get; set; } = string.Empty;
    [Required, StringLength(3000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class UpdateComplaintStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? Remarks { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class ComplaintHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string ComplaintId { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ComplaintResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string DepartmentId { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ComplaintHistoryDto> History { get; set; } = [];
}
