using MongoDB.Bson;
using MongoDB.Driver;
using SmartCampus.Data;
using SmartCampus.DTOs;
using SmartCampus.Models;

namespace SmartCampus.Services;

public class AnnouncementService(MongoDbContext context) : IAnnouncementService
{
    public async Task<List<AnnouncementResponseDto>> GetAnnouncementsAsync(bool activeOnly = false, string? search = null)
    {
        var b = Builders<Announcement>.Filter;
        var filter = b.Empty;
        if (activeOnly) filter &= b.Eq(a => a.IsActive, true);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var reg = new BsonRegularExpression(search.Trim().ToLower(), "i");
            filter &= b.Or(b.Regex(a => a.Title, reg), b.Regex(a => a.Description, reg), b.Regex(a => a.Category, reg));
        }

        var list = await context.Announcements.Find(filter).SortByDescending(a => a.PublishedAt).ToListAsync();
        return list.Select(MapAnnouncement).ToList();
    }

    public async Task<AnnouncementResponseDto?> GetAnnouncementByIdAsync(string id) =>
        await context.Announcements.Find(a => a.Id == id).FirstOrDefaultAsync() is { } a ? MapAnnouncement(a) : null;

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
        await context.Announcements.InsertOneAsync(announcement);

        var users = await context.Users.Find(u => u.IsActive && u.Role != UserRoles.Admin).Limit(50).ToListAsync();
        var notifs = users.Select(u => new Notification
        {
            UserId = u.Id,
            Title = "New Announcement",
            Message = $"{announcement.Title}: {announcement.Description[..Math.Min(80, announcement.Description.Length)]}...",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifs.Count > 0) await context.Notifications.InsertManyAsync(notifs);
        return MapAnnouncement(announcement);
    }

    public async Task<bool> UpdateAnnouncementAsync(string id, UpdateAnnouncementDto dto)
    {
        var update = Builders<Announcement>.Update
            .Set(a => a.Title, dto.Title.Trim())
            .Set(a => a.Description, dto.Description.Trim())
            .Set(a => a.Category, dto.Category.Trim())
            .Set(a => a.IsActive, dto.IsActive);
        return (await context.Announcements.UpdateOneAsync(a => a.Id == id, update)).ModifiedCount > 0;
    }

    public async Task<bool> DeleteAnnouncementAsync(string id) =>
        (await context.Announcements.DeleteOneAsync(a => a.Id == id)).DeletedCount > 0;

    private static AnnouncementResponseDto MapAnnouncement(Announcement a) => new()
    {
        Id = a.Id, Title = a.Title, Description = a.Description, Category = a.Category,
        PublishedBy = a.PublishedBy, PublishedAt = a.PublishedAt, IsActive = a.IsActive
    };
}
