using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(dto);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        var user = result.User!;
        return Created($"/api/users/{user.Id}", new UserResponseDto
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

    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(dto.Email, dto.Password);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = result.Error });
        }

        var user = result.User!;
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

