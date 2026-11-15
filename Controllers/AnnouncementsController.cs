using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Models;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class AnnouncementsController(IAnnouncementService announcementService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        return View(await announcementService.GetAnnouncementsAsync(activeOnly: !User.IsInRole(UserRoles.Admin), search: search));
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id) =>
        await announcementService.GetAnnouncementByIdAsync(id) is { } item ? View(item) : NotFound();

    [HttpGet, Authorize(Roles = UserRoles.Admin)]
    public IActionResult Create() => View(new CreateAnnouncementDto());

    [HttpPost, Authorize(Roles = UserRoles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAnnouncementDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await announcementService.CreateAnnouncementAsync(User.Identity?.Name ?? "Administration", dto);
        TempData["SuccessMessage"] = "Announcement published successfully and notified campus users.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Edit(string id)
    {
        if (await announcementService.GetAnnouncementByIdAsync(id) is not { } a) return NotFound();
        ViewBag.AnnouncementId = id;
        return View(new UpdateAnnouncementDto { Title = a.Title, Description = a.Description, Category = a.Category, IsActive = a.IsActive });
    }

    [HttpPost, Authorize(Roles = UserRoles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdateAnnouncementDto dto)
    {
        if (!ModelState.IsValid) { ViewBag.AnnouncementId = id; return View(dto); }
        var ok = await announcementService.UpdateAnnouncementAsync(id, dto);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Announcement updated successfully." : "Failed to update announcement.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = UserRoles.Admin), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var ok = await announcementService.DeleteAnnouncementAsync(id);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Announcement deleted successfully." : "Failed to delete announcement.";
        return RedirectToAction(nameof(Index));
    }
}
