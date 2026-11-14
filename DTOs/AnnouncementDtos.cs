using System.ComponentModel.DataAnnotations;

namespace SmartCampus.DTOs;

public class CreateAnnouncementDto
{
    [Required, StringLength(140, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(3000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "General";

    public bool IsActive { get; set; } = true;
}

public class UpdateAnnouncementDto : CreateAnnouncementDto
{
}

public class AnnouncementResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PublishedBy { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public bool IsActive { get; set; }
}

public class NotificationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

