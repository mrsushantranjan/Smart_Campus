using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(string userId, string title, string message);
    Task<List<NotificationResponseDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int limit = 10);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> MarkAsReadAsync(string notificationId, string userId);
    Task<bool> MarkAllAsReadAsync(string userId);
}

