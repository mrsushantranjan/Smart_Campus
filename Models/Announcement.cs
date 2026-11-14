using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartCampus.Models;

public class Announcement
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "General";

    public string PublishedBy { get; set; } = "Admin Office";

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}

