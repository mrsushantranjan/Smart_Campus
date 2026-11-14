using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersApiController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? role)
    {
        var users = await _authService.GetAllUsersAsync(role);
        var dtos = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            DepartmentId = u.DepartmentId,
            DepartmentName = u.DepartmentName,
            StudentId = u.StudentId,
            PhoneNumber = u.PhoneNumber,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"User with ID '{id}' not found." });
        }

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.DepartmentName,
            StudentId = user.StudentId,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }
}

