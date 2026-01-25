using StoryJudge.Core.Enums;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.DTOs;

public record CreateStoryRequest(
    string Title,
    StoryType StoryType,
    string Situation,
    string Task,
    string Action,
    string Result,
    string? Reflection,
    List<string>? Tags,
    Visibility Visibility
);

public record UpdateStoryRequest(
    string? Title,
    StoryType? StoryType,
    string? Situation,
    string? Task,
    string? Action,
    string? Result,
    string? Reflection,
    List<string>? Tags,
    Visibility? Visibility
);

public record StoryDto(
    string Id,
    string AuthorId,
    string AuthorName,
    string? AuthorAvatarUrl,
    string Title,
    string StoryType,
    string Situation,
    string Task,
    string Action,
    string Result,
    string? Reflection,
    List<string> Tags,
    string Visibility,
    string Status,
    string? ShareToken,
    List<CoverageItem> Coverage,
    ReviewStats ReviewStats,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record StoryListDto(
    string Id,
    string AuthorId,
    string AuthorName,
    string? AuthorAvatarUrl,
    string Title,
    string StoryType,
    List<string> Tags,
    string Visibility,
    string Status,
    ReviewStats ReviewStats,
    DateTime? PublishedAt,
    DateTime CreatedAt
);

public record PublishStoryRequest(
    Visibility Visibility
);

public record UpdateCoverageRequest(
    string Key,
    bool Value
);

public record PaginatedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
