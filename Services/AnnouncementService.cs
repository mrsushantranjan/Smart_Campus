using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly MongoDbContext _context;
    private readonly INotificationService _notificationService;

    public AnnouncementService(MongoDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<AnnouncementResponseDto>> GetAnnouncementsAsync(bool activeOnly = false, string? search = null)
    {
        var builder = Builders<Announcement>.Filter;
        var filter = builder.Empty;

        if (activeOnly)
        {
            filter &= builder.Eq(a => a.IsActive, true);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            filter &= builder.Or(
                builder.Regex(a => a.Title, new MongoDB.Bson.BsonRegularExpression(searchLower, "i")),
                builder.Regex(a => a.Description, new MongoDB.Bson.BsonRegularExpression(searchLower, "i")),
                builder.Regex(a => a.Category, new MongoDB.Bson.BsonRegularExpression(searchLower, "i"))
            );
        }

        var list = await _context.Announcements.Find(filter)
            .SortByDescending(a => a.PublishedAt)
            .ToListAsync();

        return list.Select(MapAnnouncement).ToList();
    }

    public async Task<AnnouncementResponseDto?> GetAnnouncementByIdAsync(string id)
    {
        var announcement = await _context.Announcements.Find(a => a.Id == id).FirstOrDefaultAsync();
        return announcement == null ? null : MapAnnouncement(announcement);
    }

    public async Task<AnnouncementResponseDto> CreateAnnouncementAsync(string publishedBy, CreateAnnouncementDto dto)
    {
        var announcement = new Announcement
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim(),
            PublishedBy = string.IsNullOrWhiteSpace(publishedBy) ? "Administration" : publishedBy,
            PublishedAt = DateTime.UtcNow,
            IsActive = dto.IsActive
        };

        await _context.Announcements.InsertOneAsync(announcement);

        // Broadcast notification to all active students and faculty
        var users = await _context.Users.Find(u => u.IsActive && u.Role != UserRoles.Admin).Limit(50).ToListAsync();
        var notifs = users.Select(u => new Notification
        {
            UserId = u.Id,
            Title = "New Announcement",
            Message = $"{announcement.Title}: {announcement.Description.Substring(0, Math.Min(80, announcement.Description.Length))}...",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifs.Any())
        {
            await _context.Notifications.InsertManyAsync(notifs);
        }

        return MapAnnouncement(announcement);
    }

    public async Task<bool> UpdateAnnouncementAsync(string id, UpdateAnnouncementDto dto)
    {
        var update = Builders<Announcement>.Update
            .Set(a => a.Title, dto.Title.Trim())
            .Set(a => a.Description, dto.Description.Trim())
            .Set(a => a.Category, dto.Category.Trim())
            .Set(a => a.IsActive, dto.IsActive);

        var result = await _context.Announcements.UpdateOneAsync(a => a.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAnnouncementAsync(string id)
    {
        var result = await _context.Announcements.DeleteOneAsync(a => a.Id == id);
        return result.DeletedCount > 0;
    }

    private static AnnouncementResponseDto MapAnnouncement(Announcement a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        Category = a.Category,
        PublishedBy = a.PublishedBy,
        PublishedAt = a.PublishedAt,
        IsActive = a.IsActive
    };
}

