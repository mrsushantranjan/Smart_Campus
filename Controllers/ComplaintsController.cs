using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class ComplaintsController : Controller
{
    private readonly IComplaintService _complaintService;
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public ComplaintsController(IComplaintService complaintService, IAuthService authService, IWebHostEnvironment env)
    {
        _complaintService = complaintService;
        _authService = authService;
        _env = env;
    }

    public async Task<IActionResult> Index(string? status, string? departmentId, string? search)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // If student, show only their complaints. If Admin or Faculty, show all or filtered.
        string? filterUserId = (role == UserRoles.Student) ? userId : null;
        string? filterDeptId = departmentId;

        if (role == UserRoles.Faculty && string.IsNullOrWhiteSpace(departmentId))
        {
            var user = await _authService.GetUserByIdAsync(userId);
            filterDeptId = user?.DepartmentId;
        }

        ViewBag.StatusFilter = status;
        ViewBag.DepartmentFilter = departmentId;
        ViewBag.SearchFilter = search;
        ViewBag.Departments = await _authService.GetDepartmentsAsync();

        var list = await _complaintService.GetComplaintsAsync(filterUserId, status, filterDeptId, search);
        return View(list);
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Student)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = await _authService.GetDepartmentsAsync();
        return View(new CreateComplaintDto());
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Student)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateComplaintDto model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _authService.GetDepartmentsAsync();
            return View(model);
        }

        string? imageUrl = null;
        if (imageFile != null && imageFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("ImageUrl", "Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
                ViewBag.Departments = await _authService.GetDepartmentsAsync();
                return View(model);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "complaints");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            imageUrl = $"/uploads/complaints/{uniqueFileName}";
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var created = await _complaintService.CreateComplaintAsync(userId, model, imageUrl);

        TempData["SuccessMessage"] = "Your complaint has been submitted successfully.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var complaint = await _complaintService.GetComplaintByIdAsync(id);
        if (complaint == null) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (role == UserRoles.Student && complaint.UserId != userId)
        {
            return Forbid();
        }

        ViewBag.FacultyMembers = await _authService.GetAllUsersAsync(UserRoles.Faculty);
        return View(complaint);
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Faculty}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, UpdateComplaintStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid status update details.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var updatedBy = User.Identity?.Name ?? "Staff";
        var success = await _complaintService.UpdateComplaintStatusAsync(id, updatedBy, dto);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Complaint status updated successfully." : "Failed to update complaint status.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = (role == UserRoles.Admin);

        var success = await _complaintService.DeleteComplaintAsync(id, userId, isAdmin);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Complaint deleted successfully." : "Failed to delete complaint.";

        return RedirectToAction(nameof(Index));
    }
}

