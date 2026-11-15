using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/notifications"), Produces("application/json")]
public class NotificationsApiController(INotificationService notificationService) : ControllerBase
{
    private string? GetEffectiveUser(string? uid) => uid ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] string? userId, [FromQuery] bool unreadOnly = false) =>
        GetEffectiveUser(userId) is { } uid
            ? Ok(await notificationService.GetUserNotificationsAsync(uid, unreadOnly))
            : BadRequest(new { message = "UserId is required." });

    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(string id, [FromQuery] string? userId) =>
        await notificationService.MarkAsReadAsync(id, GetEffectiveUser(userId) ?? "")
            ? Ok(new { message = "Notification marked as read." })
            : NotFound(new { message = "Notification not found or access denied." });

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] string? userId)
    {
        if (GetEffectiveUser(userId) is not { } uid) return BadRequest(new { message = "UserId is required." });
        await notificationService.MarkAllAsReadAsync(uid);
        return Ok(new { message = "All notifications marked as read." });
    }
}
