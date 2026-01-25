using StoryJudge.Core.DTOs;

namespace StoryJudge.Core.Interfaces;

public interface IReviewService
{
    Task<ReviewDto?> GetByIdAsync(string id);
    Task<List<ReviewDto>> GetByStoryIdAsync(string storyId, string? currentUserId, string? shareToken);
    Task<ReviewDto> CreateAsync(string storyId, CreateReviewRequest request, string reviewerId);
    Task<ReviewDto> UpdateAsync(string id, UpdateReviewRequest request, string currentUserId);
    Task<bool> DeleteAsync(string id, string currentUserId);
    double CalculateOverallScore(CreateReviewRequest scores);
}
