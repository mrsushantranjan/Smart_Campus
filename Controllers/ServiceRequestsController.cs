using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class ServiceRequestsController(IServiceRequestService requestService) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(UserRoles.Admin);

    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? requestType)
    {
        ViewBag.StatusFilter = status;
        ViewBag.TypeFilter = requestType;
        return View(await requestService.GetServiceRequestsAsync(IsAdmin ? null : UserId, status, requestType));
    }

    [HttpGet, Authorize(Roles = $"{UserRoles.Student},{UserRoles.Faculty}")]
    public IActionResult Create(string? type) =>
        View(new CreateServiceRequestDto { RequestType = string.IsNullOrWhiteSpace(type) ? ServiceRequestTypes.IdCard : type });

    [HttpPost, Authorize(Roles = $"{UserRoles.Student},{UserRoles.Faculty}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceRequestDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var created = await requestService.CreateServiceRequestAsync(UserId, model);
        TempData["SuccessMessage"] = $"Service request '{created.RequestType}' submitted successfully.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var item = await requestService.GetServiceRequestByIdAsync(id);
        if (item == null) return NotFound();
        if (!IsAdmin && item.UserId != UserId) return Forbid();
        return View(item);
    }

    [HttpPost, Authorize(Roles = UserRoles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, UpdateServiceRequestDto dto)
    {
        if (!ModelState.IsValid) { TempData["ErrorMessage"] = "Invalid status update details."; return RedirectToAction(nameof(Details), new { id }); }
        var ok = await requestService.UpdateServiceRequestStatusAsync(id, dto.Status, dto.AdminRemarks);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? $"Service request status updated to '{dto.Status}'." : "Failed to update service request.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var ok = await requestService.DeleteServiceRequestAsync(id, UserId, IsAdmin);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Service request deleted successfully." : "Failed to delete service request.";
        return RedirectToAction(nameof(Index));
    }
}
