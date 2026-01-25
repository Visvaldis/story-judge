using StoryJudge.Core.Models;

namespace StoryJudge.Core.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(string id);
    Task<Review?> GetByStoryAndReviewerAsync(string storyId, string reviewerId);
    Task<List<Review>> GetByStoryIdAsync(string storyId);
    Task<List<Review>> GetByReviewerIdAsync(string reviewerId, int page, int pageSize);
    Task<Review> CreateAsync(Review review);
    Task<Review> UpdateAsync(Review review);
    Task<bool> DeleteAsync(string id);
    Task<bool> SetHiddenAsync(string id, bool isHidden);
    Task<int> GetCountByReviewerAsync(string reviewerId, DateTime since);
    Task<(double AverageScore, int Count, Dictionary<string, double> CategoryAverages)> GetStoryStatsAsync(string storyId);
}
