using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartCampus.Models;

public class ServiceRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonSerializer(typeof(StringOrObjectIdNonNullSerializer))]
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public string UserRole { get; set; } = UserRoles.Student;

    public string RequestType { get; set; } = ServiceRequestTypes.IDCard;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = ServiceRequestStatuses.Pending;

    public string? AdminRemarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
