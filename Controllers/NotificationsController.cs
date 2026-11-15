using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Services;

namespace SmartCampus.Controllers;

[Authorize]
public class NotificationsController(INotificationService notificationService) : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Index() =>
        View(await notificationService.GetUserNotificationsAsync(CurrentUserId, unreadOnly: false, limit: 30));

    [HttpGet]
    public async Task<IActionResult> GetUnread() =>
        Json(new { count = await notificationService.GetUnreadCountAsync(CurrentUserId), notifications = await notificationService.GetUserNotificationsAsync(CurrentUserId, unreadOnly: false, limit: 5) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(string id, string? returnUrl = null)
    {
        await notificationService.MarkAsReadAsync(id, CurrentUserId);
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead(string? returnUrl = null)
    {
        await notificationService.MarkAllAsReadAsync(CurrentUserId);
        TempData["SuccessMessage"] = "All notifications marked as read.";
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
    }
}
