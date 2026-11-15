using System.ComponentModel.DataAnnotations;

namespace SmartCampus.DTOs;

public class CreateBookDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Author { get; set; } = string.Empty;
    [Required, StringLength(50)]
    public string ISBN { get; set; } = string.Empty;
    [Required, StringLength(60)]
    public string Category { get; set; } = string.Empty;
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    [Range(1, 1000)]
    public int TotalCopies { get; set; } = 1;
}

public class UpdateBookDto : CreateBookDto
{
    [Range(0, 1000)]
    public int AvailableCopies { get; set; }
}

public class BookResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReservationDto
{
    [Required]
    public string BookId { get; set; } = string.Empty;
}

public class ReservationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string BookId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string Author { get => BookAuthor; set => BookAuthor = value; }
    public string Category { get; set; } = "General";
    public DateTime ReservationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
