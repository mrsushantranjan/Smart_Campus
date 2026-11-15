using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class AdminController(IReportService reportService, IAuthService authService) : Controller
{
    public async Task<IActionResult> Index() => View(await reportService.GetAdminDashboardDataAsync());

    public async Task<IActionResult> Users(string? role)
    {
        ViewBag.SelectedRole = role;
        return View(await authService.GetAllUsersAsync(role));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var ok = await authService.ToggleUserActiveAsync(id);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "User status updated successfully." : "Failed to update user status.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Departments() => View(await authService.GetDepartmentsAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(DepartmentDto dto)
    {
        if (!ModelState.IsValid) { TempData["ErrorMessage"] = "Department name is required."; return RedirectToAction(nameof(Departments)); }
        await authService.CreateDepartmentAsync(dto.DepartmentName, dto.Description);
        TempData["SuccessMessage"] = "Department created successfully.";
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDepartment(string id, DepartmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(id) || !ModelState.IsValid) { TempData["ErrorMessage"] = "Invalid department details."; return RedirectToAction(nameof(Departments)); }
        var ok = await authService.UpdateDepartmentAsync(id, dto.DepartmentName, dto.Description);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Department updated successfully." : "Failed to update department.";
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDepartment(string id)
    {
        var ok = await authService.DeleteDepartmentAsync(id);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Department deleted successfully." : "Failed to delete department.";
        return RedirectToAction(nameof(Departments));
    }

    public async Task<IActionResult> Reports() => View(await reportService.GetReportsDataAsync());
}
