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
    Task<(User User, string Token)> RegisterAsync(string email, string password, string displayName);
    Task<(User User, string Token)> LoginAsync(string email, string password);
    string GenerateJwtToken(User user);
    Task<UserDto?> GetCurrentUserAsync(string userId);
}
