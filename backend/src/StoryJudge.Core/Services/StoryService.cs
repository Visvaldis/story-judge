using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Services;

public class StoryService : IStoryService
{
    private readonly IStoryRepository _storyRepository;
    private readonly IUserRepository _userRepository;

    public StoryService(IStoryRepository storyRepository, IUserRepository userRepository)
    {
        _storyRepository = storyRepository;
        _userRepository = userRepository;
    }

    public async Task<StoryDto?> GetByIdAsync(string id, string? currentUserId)
    {
        var story = await _storyRepository.GetByIdAsync(id);
        if (story == null) return null;

        if (!CanAccess(story, currentUserId, null))
        {
            throw new UnauthorizedAccessException("You don't have permission to view this story");
        }

        return await ToDto(story);
    }

    public async Task<StoryDto?> GetByShareTokenAsync(string shareToken, string? currentUserId)
    {
        var story = await _storyRepository.GetByShareTokenAsync(shareToken);
        if (story == null) return null;

        if (!CanAccess(story, currentUserId, shareToken))
        {
            throw new UnauthorizedAccessException("You don't have permission to view this story");
        }

        return await ToDto(story);
    }

    public async Task<PaginatedResponse<StoryListDto>> GetMyStoriesAsync(string userId, int page, int pageSize)
    {
        var stories = await _storyRepository.GetByAuthorIdAsync(userId, page, pageSize);
        var items = new List<StoryListDto>();

        foreach (var story in stories)
        {
            items.Add(await ToListDto(story));
        }

        // Note: For pagination, we'd need a count query. Simplified for MVP.
        return new PaginatedResponse<StoryListDto>(
            items, page, pageSize, items.Count, (items.Count + pageSize - 1) / pageSize);
    }

    public async Task<PaginatedResponse<StoryListDto>> GetPublicStoriesAsync(
        int page, int pageSize, StoryType? storyType, string? tag, string sortBy)
    {
        var (stories, totalCount) = await _storyRepository.GetPublicStoriesAsync(
            page, pageSize, storyType, tag, sortBy);

        var items = new List<StoryListDto>();
        foreach (var story in stories)
        {
            items.Add(await ToListDto(story));
        }

        var totalPages = (totalCount + pageSize - 1) / pageSize;
        return new PaginatedResponse<StoryListDto>(items, page, pageSize, totalCount, totalPages);
    }

    public async Task<StoryDto> CreateAsync(CreateStoryRequest request, string authorId)
    {
        var story = new Story
        {
            AuthorId = authorId,
            Title = request.Title,
            StoryType = request.StoryType,
            Situation = request.Situation,
            Task = request.Task,
            Action = request.Action,
            Result = request.Result,
            Reflection = request.Reflection,
            Tags = request.Tags ?? new List<string>(),
            Visibility = request.Visibility,
            Status = StoryStatus.Draft
        };

        // Generate share token for unlisted stories
        if (request.Visibility == Visibility.Unlisted)
        {
            story.ShareToken = GenerateShareToken();
        }

        // Calculate initial coverage
        story.Coverage = CalculateCoverage(story);

        await _storyRepository.CreateAsync(story);
        return await ToDto(story);
    }

    public async Task<StoryDto> UpdateAsync(string id, UpdateStoryRequest request, string currentUserId)
    {
        var story = await _storyRepository.GetByIdAsync(id);
        if (story == null)
        {
            throw new KeyNotFoundException("Story not found");
        }

        if (story.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only edit your own stories");
        }

        if (request.Title != null) story.Title = request.Title;
        if (request.StoryType.HasValue) story.StoryType = request.StoryType.Value;
        if (request.Situation != null) story.Situation = request.Situation;
        if (request.Task != null) story.Task = request.Task;
        if (request.Action != null) story.Action = request.Action;
        if (request.Result != null) story.Result = request.Result;
        if (request.Reflection != null) story.Reflection = request.Reflection;
        if (request.Tags != null) story.Tags = request.Tags;

        if (request.Visibility.HasValue && request.Visibility.Value != story.Visibility)
        {
            story.Visibility = request.Visibility.Value;
            if (request.Visibility.Value == Visibility.Unlisted && story.ShareToken == null)
            {
                story.ShareToken = GenerateShareToken();
            }
        }

        // Recalculate coverage
        story.Coverage = CalculateCoverage(story);

        await _storyRepository.UpdateAsync(story);
        return await ToDto(story);
    }

    public async Task<StoryDto> PublishAsync(string id, PublishStoryRequest request, string currentUserId)
    {
        var story = await _storyRepository.GetByIdAsync(id);
        if (story == null)
        {
            throw new KeyNotFoundException("Story not found");
        }

        if (story.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only publish your own stories");
        }

        story.Status = StoryStatus.Published;
        story.PublishedAt = DateTime.UtcNow;
        story.Visibility = request.Visibility;

        if (request.Visibility == Visibility.Unlisted && story.ShareToken == null)
        {
            story.ShareToken = GenerateShareToken();
        }

        await _storyRepository.UpdateAsync(story);
        return await ToDto(story);
    }

    public async Task<bool> DeleteAsync(string id, string currentUserId)
    {
        var story = await _storyRepository.GetByIdAsync(id);
        if (story == null)
        {
            throw new KeyNotFoundException("Story not found");
        }

        if (story.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own stories");
        }

        return await _storyRepository.DeleteAsync(id);
    }

    public async Task<bool> CanAccessStoryAsync(string storyId, string? userId, string? shareToken = null)
    {
        var story = await _storyRepository.GetByIdAsync(storyId);
        if (story == null) return false;
        return CanAccess(story, userId, shareToken);
    }

    public async Task UpdateCoverageOverrideAsync(string storyId, UpdateCoverageRequest request, string currentUserId)
    {
        var story = await _storyRepository.GetByIdAsync(storyId);
        if (story == null)
        {
            throw new KeyNotFoundException("Story not found");
        }

        if (story.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only update coverage on your own stories");
        }

        var coverageItem = story.Coverage.FirstOrDefault(c => c.Key == request.Key);
        if (coverageItem != null)
        {
            coverageItem.ManualOverride = request.Value;
        }

        await _storyRepository.UpdateCoverageAsync(storyId, story.Coverage);
    }

    private bool CanAccess(Story story, string? userId, string? shareToken)
    {
        // Author always has access
        if (!string.IsNullOrEmpty(userId) && story.AuthorId == userId)
        {
            return true;
        }

        return story.Visibility switch
        {
            Visibility.Public => story.Status == StoryStatus.Published,
            Visibility.Unlisted => !string.IsNullOrEmpty(shareToken) && story.ShareToken == shareToken,
            Visibility.Private => false,
            _ => false
        };
    }

    private static string GenerateShareToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }

    private List<CoverageItem> CalculateCoverage(Story story)
    {
        return new List<CoverageItem>
        {
            new() { Key = "has_situation", Label = "Has Situation", Detected = !string.IsNullOrWhiteSpace(story.Situation) },
            new() { Key = "has_task", Label = "Has Task", Detected = !string.IsNullOrWhiteSpace(story.Task) },
            new() { Key = "has_action", Label = "Has Action", Detected = !string.IsNullOrWhiteSpace(story.Action) },
            new() { Key = "has_result", Label = "Has Result", Detected = !string.IsNullOrWhiteSpace(story.Result) },
            new() { Key = "has_reflection", Label = "Has Reflection", Detected = !string.IsNullOrWhiteSpace(story.Reflection) },
            new() { Key = "has_metrics", Label = "Has Metrics", Detected = HasMetrics(story) },
            new() { Key = "uses_i_language", Label = "Uses 'I' Language", Detected = UsesILanguage(story) },
            new() { Key = "mentions_constraints", Label = "Mentions Constraints", Detected = MentionsConstraints(story) }
        };
    }

    private static bool HasMetrics(Story story)
    {
        var text = $"{story.Situation} {story.Task} {story.Action} {story.Result}";
        // Simple regex to detect numbers with % or numeric patterns
        return System.Text.RegularExpressions.Regex.IsMatch(text, @"\d+%|\d+x|\$\d+|\d+ (percent|times|users|customers|revenue)");
    }

    private static bool UsesILanguage(Story story)
    {
        var text = $"{story.Action} {story.Result}";
        // Check for first-person pronouns
        return System.Text.RegularExpressions.Regex.IsMatch(text, @"\b(I|my|me|mine)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static bool MentionsConstraints(Story story)
    {
        var text = $"{story.Situation} {story.Task}";
        // Check for constraint-related words
        return System.Text.RegularExpressions.Regex.IsMatch(text,
            @"\b(deadline|budget|limited|constraint|restriction|challenge|obstacle|despite|although)\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private async Task<StoryDto> ToDto(Story story)
    {
        var author = await _userRepository.GetByIdAsync(story.AuthorId);
        return new StoryDto(
            story.Id,
            story.AuthorId,
            author?.DisplayName ?? "Unknown",
            author?.AvatarUrl,
            story.Title,
            story.StoryType.ToString(),
            story.Situation,
            story.Task,
            story.Action,
            story.Result,
            story.Reflection,
            story.Tags,
            story.Visibility.ToString(),
            story.Status.ToString(),
            story.ShareToken,
            story.Coverage,
            story.ReviewStats,
            story.PublishedAt,
            story.CreatedAt,
            story.UpdatedAt
        );
    }

    private async Task<StoryListDto> ToListDto(Story story)
    {
        var author = await _userRepository.GetByIdAsync(story.AuthorId);
        return new StoryListDto(
            story.Id,
            story.AuthorId,
            author?.DisplayName ?? "Unknown",
            author?.AvatarUrl,
            story.Title,
            story.StoryType.ToString(),
            story.Tags,
            story.Visibility.ToString(),
            story.Status.ToString(),
            story.ReviewStats,
            story.PublishedAt,
            story.CreatedAt
        );
    }
}
