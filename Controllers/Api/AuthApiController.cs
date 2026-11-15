using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/auth"), Produces("application/json")]
public class AuthApiController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var res = await authService.RegisterAsync(dto);
        if (!res.Succeeded) return BadRequest(new { message = res.Error });
        return Created($"/api/users/{res.User!.Id}", ToDto(res.User));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var res = await authService.LoginAsync(dto.Email, dto.Password);
        if (!res.Succeeded) return Unauthorized(new { message = res.Error });
        return Ok(ToDto(res.User!));
    }

    private static UserResponseDto ToDto(ApplicationUser u) => new()
    {
        Id = u.Id, FullName = u.FullName, Email = u.Email, Role = u.Role,
        DepartmentId = u.DepartmentId, DepartmentName = u.DepartmentName,
        StudentId = u.StudentId, PhoneNumber = u.PhoneNumber, IsActive = u.IsActive, CreatedAt = u.CreatedAt
    };
}
