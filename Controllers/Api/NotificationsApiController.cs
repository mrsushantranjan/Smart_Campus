using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsApiController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsApiController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] string? userId, [FromQuery] bool unreadOnly = false)
    {
        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(effectiveUserId))
        {
            return BadRequest(new { message = "UserId is required (either via authentication cookie or userId query parameter)." });
        }

        var list = await _notificationService.GetUserNotificationsAsync(effectiveUserId, unreadOnly);
        return Ok(list);
    }

    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(string id, [FromQuery] string? userId)
    {
        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var success = await _notificationService.MarkAsReadAsync(id, effectiveUserId);
        if (!success)
        {
            return NotFound(new { message = "Notification not found or access denied." });
        }
        return Ok(new { message = "Notification marked as read." });
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] string? userId)
    {
        var effectiveUserId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(effectiveUserId))
        {
            return BadRequest(new { message = "UserId is required." });
        }

        await _notificationService.MarkAllAsReadAsync(effectiveUserId);
        return Ok(new { message = "All notifications marked as read." });
    }
}

