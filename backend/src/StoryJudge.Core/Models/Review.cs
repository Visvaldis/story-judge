using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StoryJudge.Core.Models;

public class Review
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("storyId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string StoryId { get; set; } = null!;

    [BsonElement("reviewerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ReviewerId { get; set; } = null!;

    [BsonElement("scores")]
    public ReviewScores Scores { get; set; } = new();

    [BsonElement("overallScore")]
    public double OverallScore { get; set; }

    [BsonElement("feedback")]
    public string? Feedback { get; set; }

    [BsonElement("strengths")]
    public List<string> Strengths { get; set; } = new();

    [BsonElement("improvements")]
    public List<string> Improvements { get; set; } = new();

    [BsonElement("isHidden")]
    public bool IsHidden { get; set; } = false;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ReviewScores
{
    [BsonElement("clarity")]
    public int Clarity { get; set; }

    [BsonElement("ownership")]
    public int Ownership { get; set; }

    [BsonElement("impact")]
    public int Impact { get; set; }

    [BsonElement("decisionMaking")]
    public int DecisionMaking { get; set; }

    [BsonElement("communication")]
    public int Communication { get; set; }

    [BsonElement("reflection")]
    public int Reflection { get; set; }

    [BsonElement("technicalDepth")]
    public int? TechnicalDepth { get; set; }
}
