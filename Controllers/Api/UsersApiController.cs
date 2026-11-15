using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/users"), Produces("application/json")]
public class UsersApiController(IAuthService authService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? role) =>
        Ok((await authService.GetAllUsersAsync(role)).Select(ToDto).ToList());

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id) =>
        await authService.GetUserByIdAsync(id) is { } u ? Ok(ToDto(u)) : NotFound(new { message = $"User with ID '{id}' not found." });

    private static UserResponseDto ToDto(ApplicationUser u) => new()
    {
        Id = u.Id, FullName = u.FullName, Email = u.Email, Role = u.Role,
        DepartmentId = u.DepartmentId, DepartmentName = u.DepartmentName,
        StudentId = u.StudentId, PhoneNumber = u.PhoneNumber, IsActive = u.IsActive, CreatedAt = u.CreatedAt
    };
}
