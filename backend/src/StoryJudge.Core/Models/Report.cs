using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StoryJudge.Core.Enums;

namespace StoryJudge.Core.Models;

public class Report
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("reporterId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ReporterId { get; set; } = null!;

    [BsonElement("targetType")]
    [BsonRepresentation(BsonType.String)]
    public ReportTargetType TargetType { get; set; }

    [BsonElement("targetId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string TargetId { get; set; } = null!;

    [BsonElement("reason")]
    [BsonRepresentation(BsonType.String)]
    public ReportReason Reason { get; set; }

    [BsonElement("details")]
    public string? Details { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    [BsonElement("resolvedById")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ResolvedById { get; set; }

    [BsonElement("resolution")]
    public string? Resolution { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }
}
