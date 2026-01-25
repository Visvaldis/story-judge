using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Interfaces;

public interface IStoryService
{
    Task<StoryDto?> GetByIdAsync(string id, string? currentUserId);
    Task<StoryDto?> GetByShareTokenAsync(string shareToken, string? currentUserId);
    Task<PaginatedResponse<StoryListDto>> GetMyStoriesAsync(string userId, int page, int pageSize);
    Task<PaginatedResponse<StoryListDto>> GetPublicStoriesAsync(
        int page, int pageSize, StoryType? storyType, string? tag, string sortBy);
    Task<StoryDto> CreateAsync(CreateStoryRequest request, string authorId);
    Task<StoryDto> UpdateAsync(string id, UpdateStoryRequest request, string currentUserId);
    Task<StoryDto> PublishAsync(string id, PublishStoryRequest request, string currentUserId);
    Task<bool> DeleteAsync(string id, string currentUserId);
    Task<bool> CanAccessStoryAsync(string storyId, string? userId, string? shareToken = null);
    Task UpdateCoverageOverrideAsync(string storyId, UpdateCoverageRequest request, string currentUserId);
}
