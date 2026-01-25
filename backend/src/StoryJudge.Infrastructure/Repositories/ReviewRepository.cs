using MongoDB.Driver;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;
using StoryJudge.Infrastructure.Data;

namespace StoryJudge.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly MongoDbContext _context;

    public ReviewRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(string id)
    {
        return await _context.Reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Review?> GetByStoryAndReviewerAsync(string storyId, string reviewerId)
    {
        return await _context.Reviews
            .Find(r => r.StoryId == storyId && r.ReviewerId == reviewerId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Review>> GetByStoryIdAsync(string storyId)
    {
        return await _context.Reviews
            .Find(r => r.StoryId == storyId && !r.IsHidden)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByReviewerIdAsync(string reviewerId, int page, int pageSize)
    {
        return await _context.Reviews
            .Find(r => r.ReviewerId == reviewerId)
            .SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Review> CreateAsync(Review review)
    {
        await _context.Reviews.InsertOneAsync(review);
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        review.UpdatedAt = DateTime.UtcNow;
        await _context.Reviews.ReplaceOneAsync(r => r.Id == review.Id, review);
        return review;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.Reviews.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> SetHiddenAsync(string id, bool isHidden)
    {
        var update = Builders<Review>.Update
            .Set(r => r.IsHidden, isHidden)
            .Set(r => r.UpdatedAt, DateTime.UtcNow);
        var result = await _context.Reviews.UpdateOneAsync(r => r.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<int> GetCountByReviewerAsync(string reviewerId, DateTime since)
    {
        var count = await _context.Reviews
            .CountDocumentsAsync(r => r.ReviewerId == reviewerId && r.CreatedAt >= since);
        return (int)count;
    }

    public async Task<(double AverageScore, int Count, Dictionary<string, double> CategoryAverages)> GetStoryStatsAsync(string storyId)
    {
        var reviews = await _context.Reviews
            .Find(r => r.StoryId == storyId && !r.IsHidden)
            .ToListAsync();

        if (reviews.Count == 0)
        {
            return (0, 0, new Dictionary<string, double>());
        }

        var averageScore = reviews.Average(r => r.OverallScore);
        var categoryAverages = new Dictionary<string, double>
        {
            ["clarity"] = reviews.Average(r => r.Scores.Clarity),
            ["ownership"] = reviews.Average(r => r.Scores.Ownership),
            ["impact"] = reviews.Average(r => r.Scores.Impact),
            ["decisionMaking"] = reviews.Average(r => r.Scores.DecisionMaking),
            ["communication"] = reviews.Average(r => r.Scores.Communication),
            ["reflection"] = reviews.Average(r => r.Scores.Reflection)
        };

        var techReviews = reviews.Where(r => r.Scores.TechnicalDepth.HasValue).ToList();
        if (techReviews.Count > 0)
        {
            categoryAverages["technicalDepth"] = techReviews.Average(r => r.Scores.TechnicalDepth!.Value);
        }

        return (averageScore, reviews.Count, categoryAverages);
    }
}
