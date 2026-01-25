using StoryJudge.Core.Models;

namespace StoryJudge.Core.DTOs;

public record CreateReviewRequest(
    int Clarity,
    int Ownership,
    int Impact,
    int DecisionMaking,
    int Communication,
    int Reflection,
    int? TechnicalDepth,
    string? Feedback,
    List<string>? Strengths,
    List<string>? Improvements
);

public record UpdateReviewRequest(
    int? Clarity,
    int? Ownership,
    int? Impact,
    int? DecisionMaking,
    int? Communication,
    int? Reflection,
    int? TechnicalDepth,
    string? Feedback,
    List<string>? Strengths,
    List<string>? Improvements
);

public record ReviewDto(
    string Id,
    string StoryId,
    string ReviewerId,
    string ReviewerName,
    string? ReviewerAvatarUrl,
    ReviewScores Scores,
    double OverallScore,
    string? Feedback,
    List<string> Strengths,
    List<string> Improvements,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
