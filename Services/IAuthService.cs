using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public interface IAuthService
{
    Task<(bool Succeeded, string? Error, ApplicationUser? User)> RegisterAsync(RegisterRequestDto dto);
    Task<(bool Succeeded, string? Error, ApplicationUser? User)> LoginAsync(string email, string password);
    Task<ApplicationUser?> GetUserByIdAsync(string id);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<List<ApplicationUser>> GetAllUsersAsync(string? role = null);
    Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<bool> ToggleUserActiveAsync(string userId);
    Task<List<Department>> GetDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(string id);
    Task<Department> CreateDepartmentAsync(string name, string description);
    Task<bool> UpdateDepartmentAsync(string id, string name, string description);
    Task<bool> DeleteDepartmentAsync(string id);
}

