using System.ComponentModel.DataAnnotations;

namespace SmartCampus.DTOs;

public class RegisterRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }

    public string Role { get; set; } = "Student";

    public string? DepartmentId { get; set; }

    public string? StudentId { get; set; }

    public string? PhoneNumber { get; set; }
}

public class RegisterDto : RegisterRequestDto { }

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class LoginDto : LoginRequestDto { }

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? StudentId { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? StudentId { get; set; }

    public string? DepartmentId { get; set; }
}

public class DepartmentDto
{
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
