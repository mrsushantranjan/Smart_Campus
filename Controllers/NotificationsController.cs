using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifs = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: false, limit: 30);
        return View(notifs);
    }

    [HttpGet]
    public async Task<IActionResult> GetUnread()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var count = await _notificationService.GetUnreadCountAsync(userId);
        var notifs = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: false, limit: 5);
        return Json(new { count, notifications = notifs });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(string id, string? returnUrl = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.MarkAsReadAsync(id, userId);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead(string? returnUrl = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.MarkAllAsReadAsync(userId);

        TempData["SuccessMessage"] = "All notifications marked as read.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}

