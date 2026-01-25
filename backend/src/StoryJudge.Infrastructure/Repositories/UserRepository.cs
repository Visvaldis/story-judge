using MongoDB.Driver;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;
using StoryJudge.Infrastructure.Data;

namespace StoryJudge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByOAuthAsync(OAuthProvider provider, string oauthId)
    {
        return await _context.Users
            .Find(u => u.OAuthProvider == provider && u.OAuthId == oauthId)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.InsertOneAsync(user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
        return user;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> SetBannedAsync(string id, bool isBanned)
    {
        var update = Builders<User>.Update
            .Set(u => u.IsBanned, isBanned)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
        return result.ModifiedCount > 0;
    }
}
