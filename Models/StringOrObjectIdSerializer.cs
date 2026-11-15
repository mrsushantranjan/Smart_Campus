using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SmartCampus.Models;

public class StringOrObjectIdSerializer : SerializerBase<string?>
{
    public override string? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.CurrentBsonType;
        if (type == BsonType.ObjectId) return context.Reader.ReadObjectId().ToString();
        if (type == BsonType.String) return context.Reader.ReadString();
        if (type == BsonType.Null) context.Reader.ReadNull(); else context.Reader.SkipValue();
        return null;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) context.Writer.WriteNull();
        else if (ObjectId.TryParse(value, out var oid)) context.Writer.WriteObjectId(oid);
        else context.Writer.WriteString(value);
    }
}

public class StringOrObjectIdNonNullSerializer : SerializerBase<string>
{
    public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.CurrentBsonType;
        if (type == BsonType.ObjectId) return context.Reader.ReadObjectId().ToString();
        if (type == BsonType.String) return context.Reader.ReadString();
        if (type == BsonType.Null) context.Reader.ReadNull(); else context.Reader.SkipValue();
        return string.Empty;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) context.Writer.WriteString(string.Empty);
        else if (ObjectId.TryParse(value, out var oid)) context.Writer.WriteObjectId(oid);
        else context.Writer.WriteString(value);
    }
}
