using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class AdminController : Controller
{
    private readonly IReportService _reportService;
    private readonly IAuthService _authService;

    public AdminController(IReportService reportService, IAuthService authService)
    {
        _reportService = reportService;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _reportService.GetAdminDashboardDataAsync();
        return View(data);
    }

    public async Task<IActionResult> Users(string? role)
    {
        ViewBag.SelectedRole = role;
        var users = await _authService.GetAllUsersAsync(role);
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var success = await _authService.ToggleUserActiveAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "User status updated successfully." : "Failed to update user status.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Departments()
    {
        var depts = await _authService.GetDepartmentsAsync();
        return View(depts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(DepartmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Department name is required.";
            return RedirectToAction(nameof(Departments));
        }

        await _authService.CreateDepartmentAsync(dto.DepartmentName, dto.Description);
        TempData["SuccessMessage"] = "Department created successfully.";
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDepartment(string id, DepartmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(id) || !ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid department details.";
            return RedirectToAction(nameof(Departments));
        }

        var success = await _authService.UpdateDepartmentAsync(id, dto.DepartmentName, dto.Description);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Department updated successfully." : "Failed to update department.";
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDepartment(string id)
    {
        var success = await _authService.DeleteDepartmentAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Department deleted successfully." : "Failed to delete department.";
        return RedirectToAction(nameof(Departments));
    }

    public async Task<IActionResult> Reports()
    {
        var data = await _reportService.GetReportsDataAsync();
        return View(data);
    }
}

