using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartCampus.Models;

public class Reservation
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [BsonSerializer(typeof(StringOrObjectIdNonNullSerializer))]
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    [BsonSerializer(typeof(StringOrObjectIdNonNullSerializer))]
    public string BookId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = ReservationStatuses.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
