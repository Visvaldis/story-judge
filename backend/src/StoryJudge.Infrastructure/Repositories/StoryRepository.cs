using MongoDB.Driver;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;
using StoryJudge.Infrastructure.Data;

namespace StoryJudge.Infrastructure.Repositories;

public class StoryRepository : IStoryRepository
{
    private readonly MongoDbContext _context;

    public StoryRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Story?> GetByIdAsync(string id)
    {
        return await _context.Stories.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Story?> GetByShareTokenAsync(string shareToken)
    {
        return await _context.Stories.Find(s => s.ShareToken == shareToken).FirstOrDefaultAsync();
    }

    public async Task<List<Story>> GetByAuthorIdAsync(string authorId, int page, int pageSize)
    {
        return await _context.Stories
            .Find(s => s.AuthorId == authorId)
            .SortByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<(List<Story> Items, int TotalCount)> GetPublicStoriesAsync(
        int page,
        int pageSize,
        StoryType? storyType = null,
        string? tag = null,
        string sortBy = "newest")
    {
        var filterBuilder = Builders<Story>.Filter;
        var filter = filterBuilder.Eq(s => s.Visibility, Visibility.Public) &
                     filterBuilder.Eq(s => s.Status, StoryStatus.Published);

        if (storyType.HasValue)
        {
            filter &= filterBuilder.Eq(s => s.StoryType, storyType.Value);
        }

        if (!string.IsNullOrEmpty(tag))
        {
            filter &= filterBuilder.AnyEq(s => s.Tags, tag);
        }

        var totalCount = await _context.Stories.CountDocumentsAsync(filter);

        var query = _context.Stories.Find(filter);

        query = sortBy switch
        {
            "highest-rated" => query.SortByDescending(s => s.ReviewStats.OverallScore),
            "most-reviewed" => query.SortByDescending(s => s.ReviewStats.TotalReviews),
            _ => query.SortByDescending(s => s.PublishedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (items, (int)totalCount);
    }

    public async Task<Story> CreateAsync(Story story)
    {
        await _context.Stories.InsertOneAsync(story);
        return story;
    }

    public async Task<Story> UpdateAsync(Story story)
    {
        story.UpdatedAt = DateTime.UtcNow;
        await _context.Stories.ReplaceOneAsync(s => s.Id == story.Id, story);
        return story;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.Stories.DeleteOneAsync(s => s.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task UpdateReviewStatsAsync(string storyId, ReviewStats stats)
    {
        var update = Builders<Story>.Update
            .Set(s => s.ReviewStats, stats)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);
        await _context.Stories.UpdateOneAsync(s => s.Id == storyId, update);
    }

    public async Task UpdateCoverageAsync(string storyId, List<CoverageItem> coverage)
    {
        var update = Builders<Story>.Update
            .Set(s => s.Coverage, coverage)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);
        await _context.Stories.UpdateOneAsync(s => s.Id == storyId, update);
    }

    public async Task<int> GetCountByAuthorAsync(string authorId, DateTime since)
    {
        var count = await _context.Stories
            .CountDocumentsAsync(s => s.AuthorId == authorId && s.CreatedAt >= since);
        return (int)count;
    }
}
