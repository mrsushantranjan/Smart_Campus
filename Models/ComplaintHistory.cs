using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartCampus.Models;

public class ComplaintHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonSerializer(typeof(StringOrObjectIdNonNullSerializer))]
    public string ComplaintId { get; set; } = string.Empty;

    public string UpdatedBy { get; set; } = string.Empty;

    public string PreviousStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

