using StoryJudge.Core.Enums;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Interfaces;

public interface IStoryRepository
{
    Task<Story?> GetByIdAsync(string id);
    Task<Story?> GetByShareTokenAsync(string shareToken);
    Task<List<Story>> GetByAuthorIdAsync(string authorId, int page, int pageSize);
    Task<(List<Story> Items, int TotalCount)> GetPublicStoriesAsync(
        int page,
        int pageSize,
        StoryType? storyType = null,
        string? tag = null,
        string sortBy = "newest");
    Task<Story> CreateAsync(Story story);
    Task<Story> UpdateAsync(Story story);
    Task<bool> DeleteAsync(string id);
    Task UpdateReviewStatsAsync(string storyId, ReviewStats stats);
    Task UpdateCoverageAsync(string storyId, List<CoverageItem> coverage);
    Task<int> GetCountByAuthorAsync(string authorId, DateTime since);
}
