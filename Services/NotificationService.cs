using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class NotificationService : INotificationService
{
    private readonly MongoDbContext _context;

    public NotificationService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateNotificationAsync(string userId, string title, string message)
    {
        var notif = new Notification
        {
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Notifications.InsertOneAsync(notif);
        return notif;
    }

    public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int limit = 10)
    {
        var builder = Builders<Notification>.Filter;
        var filter = builder.Eq(n => n.UserId, userId);

        if (unreadOnly)
        {
            filter &= builder.Eq(n => n.IsRead, false);
        }

        var list = await _context.Notifications.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Limit(limit)
            .ToListAsync();

        return list.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var count = await _context.Notifications.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);
        return (int)count;
    }

    public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
    {
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        var result = await _context.Notifications.UpdateOneAsync(n => n.Id == notificationId && n.UserId == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        var result = await _context.Notifications.UpdateManyAsync(n => n.UserId == userId && !n.IsRead, update);
        return result.ModifiedCount > 0;
    }
}

