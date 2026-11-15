using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class ComplaintsController(IComplaintService complaintService, IAuthService authService, IWebHostEnvironment env) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string Role => User.FindFirstValue(ClaimTypes.Role)!;

    public async Task<IActionResult> Index(string? status, string? departmentId, string? search)
    {
        string? filterDeptId = departmentId;
        if (Role == UserRoles.Faculty && string.IsNullOrWhiteSpace(departmentId))
            filterDeptId = (await authService.GetUserByIdAsync(UserId))?.DepartmentId;

        ViewBag.StatusFilter = status;
        ViewBag.DepartmentFilter = departmentId;
        ViewBag.SearchFilter = search;
        ViewBag.Departments = await authService.GetDepartmentsAsync();

        return View(await complaintService.GetComplaintsAsync(Role == UserRoles.Student ? UserId : null, status, filterDeptId, search));
    }

    [HttpGet, Authorize(Roles = UserRoles.Student)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = await authService.GetDepartmentsAsync();
        return View(new CreateComplaintDto());
    }

    [HttpPost, Authorize(Roles = UserRoles.Student), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateComplaintDto model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) { ViewBag.Departments = await authService.GetDepartmentsAsync(); return View(model); }

        string? imageUrl = null;
        if (imageFile is { Length: > 0 })
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            {
                ModelState.AddModelError("ImageUrl", "Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
                ViewBag.Departments = await authService.GetDepartmentsAsync();
                return View(model);
            }

            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "complaints");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            await using var stream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
            await imageFile.CopyToAsync(stream);
            imageUrl = $"/uploads/complaints/{uniqueFileName}";
        }

        var created = await complaintService.CreateComplaintAsync(UserId, model, imageUrl);
        TempData["SuccessMessage"] = "Your complaint has been submitted successfully.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var complaint = await complaintService.GetComplaintByIdAsync(id);
        if (complaint == null) return NotFound();
        if (Role == UserRoles.Student && complaint.UserId != UserId) return Forbid();

        ViewBag.FacultyMembers = await authService.GetAllUsersAsync(UserRoles.Faculty);
        return View(complaint);
    }

    [HttpPost, Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Faculty}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, UpdateComplaintStatusDto dto)
    {
        if (!ModelState.IsValid) { TempData["ErrorMessage"] = "Invalid status update details."; return RedirectToAction(nameof(Details), new { id }); }
        var ok = await complaintService.UpdateComplaintStatusAsync(id, User.Identity?.Name ?? "Staff", dto);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Complaint status updated successfully." : "Failed to update complaint status.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var ok = await complaintService.DeleteComplaintAsync(id, UserId, Role == UserRoles.Admin);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Complaint deleted successfully." : "Failed to delete complaint.";
        return RedirectToAction(nameof(Index));
    }
}
