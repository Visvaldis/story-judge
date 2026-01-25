using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StoryJudge.Core.Enums;

namespace StoryJudge.Core.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = null!;

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("roleLevel")]
    [BsonRepresentation(BsonType.String)]
    public RoleLevel RoleLevel { get; set; } = RoleLevel.User;

    [BsonElement("oauthProvider")]
    [BsonRepresentation(BsonType.String)]
    public OAuthProvider OAuthProvider { get; set; }

    [BsonElement("oauthId")]
    public string OAuthId { get; set; } = null!;

    [BsonElement("isBanned")]
    public bool IsBanned { get; set; } = false;

    [BsonElement("reputationScore")]
    public int ReputationScore { get; set; } = 0;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
