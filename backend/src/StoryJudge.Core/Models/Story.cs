using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StoryJudge.Core.Enums;

namespace StoryJudge.Core.Models;

public class Story
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("authorId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string AuthorId { get; set; } = null!;

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("storyType")]
    [BsonRepresentation(BsonType.String)]
    public StoryType StoryType { get; set; }

    [BsonElement("situation")]
    public string Situation { get; set; } = string.Empty;

    [BsonElement("task")]
    public string Task { get; set; } = string.Empty;

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    [BsonElement("result")]
    public string Result { get; set; } = string.Empty;

    [BsonElement("reflection")]
    public string? Reflection { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("visibility")]
    [BsonRepresentation(BsonType.String)]
    public Visibility Visibility { get; set; } = Visibility.Private;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public StoryStatus Status { get; set; } = StoryStatus.Draft;

    [BsonElement("shareToken")]
    public string? ShareToken { get; set; }

    [BsonElement("coverage")]
    public List<CoverageItem> Coverage { get; set; } = new();

    [BsonElement("reviewStats")]
    public ReviewStats ReviewStats { get; set; } = new();

    [BsonElement("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CoverageItem
{
    [BsonElement("key")]
    public string Key { get; set; } = null!;

    [BsonElement("label")]
    public string Label { get; set; } = null!;

    [BsonElement("detected")]
    public bool Detected { get; set; }

    [BsonElement("manualOverride")]
    public bool? ManualOverride { get; set; }
}

public class ReviewStats
{
    [BsonElement("totalReviews")]
    public int TotalReviews { get; set; } = 0;

    [BsonElement("overallScore")]
    public double OverallScore { get; set; } = 0;

    [BsonElement("categoryAverages")]
    public Dictionary<string, double> CategoryAverages { get; set; } = new();
}
