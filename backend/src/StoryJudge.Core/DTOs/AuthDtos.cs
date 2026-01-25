namespace StoryJudge.Core.DTOs;

public record LoginResponse(string Token, UserDto User);

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string? Username,
    string? AvatarUrl,
    string? Bio,
    string RoleLevel,
    int ReputationScore,
    DateTime CreatedAt
);

public record UpdateUserRequest(
    string? DisplayName,
    string? Username,
    string? Bio
);
