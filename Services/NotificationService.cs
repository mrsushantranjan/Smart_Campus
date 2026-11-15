using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class NotificationService(MongoDbContext context) : INotificationService
{
    public async Task<Notification> CreateNotificationAsync(string userId, string title, string message)
    {
        var notif = new Notification { UserId = userId, Title = title.Trim(), Message = message.Trim(), IsRead = false, CreatedAt = DateTime.UtcNow };
        await context.Notifications.InsertOneAsync(notif);
        return notif;
    }

    public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int limit = 10)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
        if (unreadOnly) filter &= Builders<Notification>.Filter.Eq(n => n.IsRead, false);

        var list = await context.Notifications.Find(filter).SortByDescending(n => n.CreatedAt).Limit(limit).ToListAsync();
        return list.Select(n => new NotificationResponseDto
        {
            Id = n.Id, UserId = n.UserId, Title = n.Title, Message = n.Message, IsRead = n.IsRead, CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId) =>
        (int)await context.Notifications.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<bool> MarkAsReadAsync(string notificationId, string userId) =>
        (await context.Notifications.UpdateOneAsync(n => n.Id == notificationId && n.UserId == userId,
            Builders<Notification>.Update.Set(n => n.IsRead, true))).ModifiedCount > 0;

    public async Task<bool> MarkAllAsReadAsync(string userId) =>
        (await context.Notifications.UpdateManyAsync(n => n.UserId == userId && !n.IsRead,
            Builders<Notification>.Update.Set(n => n.IsRead, true))).ModifiedCount > 0;
}
