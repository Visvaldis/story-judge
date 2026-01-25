using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Interfaces;

public interface IAuthService
{
    Task<(User User, string Token)> AuthenticateOAuthAsync(
        OAuthProvider provider,
        string oauthId,
        string email,
        string displayName,
        string? avatarUrl);
    string GenerateJwtToken(User user);
    Task<UserDto?> GetCurrentUserAsync(string userId);
}
