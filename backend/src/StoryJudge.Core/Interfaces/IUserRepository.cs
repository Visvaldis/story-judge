using StoryJudge.Core.Enums;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByOAuthAsync(OAuthProvider provider, string oauthId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    Task<bool> SetBannedAsync(string id, bool isBanned);
}
