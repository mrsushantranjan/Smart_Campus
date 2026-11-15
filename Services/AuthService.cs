using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class AuthService(MongoDbContext context) : IAuthService
{
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public async Task<(bool Succeeded, string? Error, ApplicationUser? User)> RegisterAsync(RegisterRequestDto dto)
    {
        var cleanEmail = dto.Email.Trim().ToLowerInvariant();
        if (await context.Users.Find(u => u.Email == cleanEmail).AnyAsync())
            return (false, "An account with this email address already exists.", null);

        string? departmentName = null;
        if (!string.IsNullOrWhiteSpace(dto.DepartmentId))
        {
            var dept = await context.Departments.Find(d => d.Id == dto.DepartmentId).FirstOrDefaultAsync();
            departmentName = dept?.DepartmentName;
        }

        var user = new ApplicationUser
        {
            FullName = dto.FullName.Trim(),
            Email = cleanEmail,
            Role = UserRoles.All.Contains(dto.Role) ? dto.Role : UserRoles.Student,
            DepartmentId = dto.DepartmentId,
            DepartmentName = departmentName,
            StudentId = dto.StudentId?.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        await context.Users.InsertOneAsync(user);
        return (true, null, user);
    }

    public async Task<(bool Succeeded, string? Error, ApplicationUser? User)> LoginAsync(string email, string password)
    {
        var cleanEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await context.Users.Find(u => u.Email == cleanEmail).FirstOrDefaultAsync();
        if (user == null) return (false, "Invalid email or password.", null);
        if (!user.IsActive) return (false, "Your account has been deactivated. Please contact the administrator.", null);

        return _hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed
            ? (false, "Invalid email or password.", null)
            : (true, null, user);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string id) =>
        !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _)
            ? await context.Users.Find(u => u.Id == id).FirstOrDefaultAsync()
            : null;

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email) =>
        await context.Users.Find(u => u.Email == (email ?? string.Empty).Trim().ToLowerInvariant()).FirstOrDefaultAsync();

    public async Task<List<ApplicationUser>> GetAllUsersAsync(string? role = null)
    {
        var filter = string.IsNullOrWhiteSpace(role)
            ? FilterDefinition<ApplicationUser>.Empty
            : Builders<ApplicationUser>.Filter.Eq(u => u.Role, role);
        return await context.Users.Find(filter).SortByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        string? departmentName = user.DepartmentName;
        if (!string.IsNullOrWhiteSpace(dto.DepartmentId))
        {
            var dept = await context.Departments.Find(d => d.Id == dto.DepartmentId).FirstOrDefaultAsync();
            departmentName = dept?.DepartmentName;
        }

        var update = Builders<ApplicationUser>.Update
            .Set(u => u.FullName, dto.FullName.Trim())
            .Set(u => u.PhoneNumber, dto.PhoneNumber?.Trim())
            .Set(u => u.StudentId, dto.StudentId?.Trim())
            .Set(u => u.DepartmentId, dto.DepartmentId)
            .Set(u => u.DepartmentName, departmentName);

        return (await context.Users.UpdateOneAsync(u => u.Id == userId, update)).ModifiedCount > 0;
    }

    public async Task<bool> ToggleUserActiveAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;
        return (await context.Users.UpdateOneAsync(u => u.Id == userId,
            Builders<ApplicationUser>.Update.Set(u => u.IsActive, !user.IsActive))).ModifiedCount > 0;
    }

    public async Task<List<Department>> GetDepartmentsAsync() =>
        await context.Departments.Find(FilterDefinition<Department>.Empty).SortBy(d => d.DepartmentName).ToListAsync();

    public async Task<Department?> GetDepartmentByIdAsync(string id) =>
        await context.Departments.Find(d => d.Id == id).FirstOrDefaultAsync();

    public async Task<Department> CreateDepartmentAsync(string name, string description)
    {
        var dept = new Department { DepartmentName = name.Trim(), Description = description.Trim(), CreatedAt = DateTime.UtcNow };
        await context.Departments.InsertOneAsync(dept);
        return dept;
    }

    public async Task<bool> UpdateDepartmentAsync(string id, string name, string description) =>
        (await context.Departments.UpdateOneAsync(d => d.Id == id, Builders<Department>.Update
            .Set(d => d.DepartmentName, name.Trim())
            .Set(d => d.Description, description.Trim()))).ModifiedCount > 0;

    public async Task<bool> DeleteDepartmentAsync(string id) =>
        (await context.Departments.DeleteOneAsync(d => d.Id == id)).DeletedCount > 0;
}
