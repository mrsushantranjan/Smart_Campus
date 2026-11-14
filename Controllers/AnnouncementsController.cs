using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class AnnouncementsController : Controller
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var isAdmin = User.IsInRole(UserRoles.Admin);
        ViewBag.Search = search;

        // Admin can see both active and inactive; Students/Faculty only see active
        var list = await _announcementService.GetAnnouncementsAsync(activeOnly: !isAdmin, search: search);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
        if (announcement == null) return NotFound();

        return View(announcement);
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult Create()
    {
        return View(new CreateAnnouncementDto());
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAnnouncementDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var publisher = User.Identity?.Name ?? "Administration";
        await _announcementService.CreateAnnouncementAsync(publisher, dto);

        TempData["SuccessMessage"] = "Announcement published successfully and notified campus users.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Edit(string id)
    {
        var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
        if (announcement == null) return NotFound();

        var model = new UpdateAnnouncementDto
        {
            Title = announcement.Title,
            Description = announcement.Description,
            Category = announcement.Category,
            IsActive = announcement.IsActive
        };
        ViewBag.AnnouncementId = id;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdateAnnouncementDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AnnouncementId = id;
            return View(dto);
        }

        var success = await _announcementService.UpdateAnnouncementAsync(id, dto);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Announcement updated successfully." : "Failed to update announcement.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _announcementService.DeleteAnnouncementAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Announcement deleted successfully." : "Failed to delete announcement.";

        return RedirectToAction(nameof(Index));
    }
}

