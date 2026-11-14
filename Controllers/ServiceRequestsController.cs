using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class ServiceRequestsController : Controller
{
    private readonly IServiceRequestService _requestService;

    public ServiceRequestsController(IServiceRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? requestType)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Admin sees all, Student/Faculty see their own
        string? filterUserId = (role == UserRoles.Admin) ? null : userId;

        ViewBag.StatusFilter = status;
        ViewBag.TypeFilter = requestType;

        var list = await _requestService.GetServiceRequestsAsync(filterUserId, status, requestType);
        return View(list);
    }

    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Student},{UserRoles.Faculty}")]
    public IActionResult Create(string? type)
    {
        return View(new CreateServiceRequestDto
        {
            RequestType = string.IsNullOrWhiteSpace(type) ? ServiceRequestTypes.IdCard : type
        });
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Student},{UserRoles.Faculty}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceRequestDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var created = await _requestService.CreateServiceRequestAsync(userId, model);

        TempData["SuccessMessage"] = $"Service request '{created.RequestType}' submitted successfully.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var item = await _requestService.GetServiceRequestByIdAsync(id);
        if (item == null) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (role != UserRoles.Admin && item.UserId != userId)
        {
            return Forbid();
        }

        return View(item);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, UpdateServiceRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid status update details.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var success = await _requestService.UpdateServiceRequestStatusAsync(id, dto.Status, dto.AdminRemarks);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? $"Service request status updated to '{dto.Status}'." : "Failed to update service request.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = (role == UserRoles.Admin);

        var success = await _requestService.DeleteServiceRequestAsync(id, userId, isAdmin);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Service request deleted successfully." : "Failed to delete service request.";

        return RedirectToAction(nameof(Index));
    }
}

