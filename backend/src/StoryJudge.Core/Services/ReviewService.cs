using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IUserRepository _userRepository;

    // Scoring weights
    private static readonly Dictionary<string, double> Weights = new()
    {
        { "clarity", 1.0 },
        { "ownership", 1.2 },
        { "impact", 1.2 },
        { "decisionMaking", 1.0 },
        { "communication", 1.0 },
        { "reflection", 0.8 },
        { "technicalDepth", 0.6 }
    };

    public ReviewService(
        IReviewRepository reviewRepository,
        IStoryRepository storyRepository,
        IUserRepository userRepository)
    {
        _reviewRepository = reviewRepository;
        _storyRepository = storyRepository;
        _userRepository = userRepository;
    }

    public async Task<ReviewDto?> GetByIdAsync(string id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null) return null;
        return await ToDto(review);
    }

    public async Task<List<ReviewDto>> GetByStoryIdAsync(string storyId, string? currentUserId, string? shareToken)
    {
        var story = await _storyRepository.GetByIdAsync(storyId);
        if (story == null)
        {
            throw new KeyNotFoundException("Story not found");
        }

        // Check access
        var hasAccess = story.AuthorId == currentUserId ||
                        (story.Visibility == Visibility.Public && story.Status == StoryStatus.Published) ||
                        (story.Visibility == Visibility.Unlisted && story.ShareToken == shareToken);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You don't have permission to view reviews for this story");
        }

        var reviews = await _reviewRepository.GetByStoryIdAsync(storyId);
        var dtos = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            dtos.Add(await ToDto(review));
        }
        return dtos;
    }

    public async Task<ReviewDto> CreateAsync(string storyId, CreateReviewRequest request, string reviewerId)
    {
        var story = await _storyRepository.GetByIdAsync(storyId);
        if (story == null)
        {
            throw new KeyNotFoundException("Story not found");
        }

        // Check if story is published and accessible
        if (story.Status != StoryStatus.Published)
        {
            throw new InvalidOperationException("Cannot review an unpublished story");
        }

        // Prevent self-review
        if (story.AuthorId == reviewerId)
        {
            throw new InvalidOperationException("You cannot review your own story");
        }

        // Check for existing review
        var existingReview = await _reviewRepository.GetByStoryAndReviewerAsync(storyId, reviewerId);
        if (existingReview != null)
        {
            throw new InvalidOperationException("You have already reviewed this story");
        }

        // Validate scores
        ValidateScores(request);

        var review = new Review
        {
            StoryId = storyId,
            ReviewerId = reviewerId,
            Scores = new ReviewScores
            {
                Clarity = request.Clarity,
                Ownership = request.Ownership,
                Impact = request.Impact,
                DecisionMaking = request.DecisionMaking,
                Communication = request.Communication,
                Reflection = request.Reflection,
                TechnicalDepth = request.TechnicalDepth
            },
            OverallScore = CalculateOverallScore(request),
            Feedback = request.Feedback,
            Strengths = request.Strengths ?? new List<string>(),
            Improvements = request.Improvements ?? new List<string>()
        };

        await _reviewRepository.CreateAsync(review);

        // Update story stats
        await UpdateStoryStats(storyId);

        return await ToDto(review);
    }

    public async Task<ReviewDto> UpdateAsync(string id, UpdateReviewRequest request, string currentUserId)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            throw new KeyNotFoundException("Review not found");
        }

        if (review.ReviewerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only edit your own reviews");
        }

        // Update scores if provided
        if (request.Clarity.HasValue) review.Scores.Clarity = request.Clarity.Value;
        if (request.Ownership.HasValue) review.Scores.Ownership = request.Ownership.Value;
        if (request.Impact.HasValue) review.Scores.Impact = request.Impact.Value;
        if (request.DecisionMaking.HasValue) review.Scores.DecisionMaking = request.DecisionMaking.Value;
        if (request.Communication.HasValue) review.Scores.Communication = request.Communication.Value;
        if (request.Reflection.HasValue) review.Scores.Reflection = request.Reflection.Value;
        if (request.TechnicalDepth.HasValue) review.Scores.TechnicalDepth = request.TechnicalDepth.Value;
        if (request.Feedback != null) review.Feedback = request.Feedback;
        if (request.Strengths != null) review.Strengths = request.Strengths;
        if (request.Improvements != null) review.Improvements = request.Improvements;

        // Recalculate overall score
        review.OverallScore = CalculateOverallScoreFromReview(review);

        await _reviewRepository.UpdateAsync(review);

        // Update story stats
        await UpdateStoryStats(review.StoryId);

        return await ToDto(review);
    }

    public async Task<bool> DeleteAsync(string id, string currentUserId)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            throw new KeyNotFoundException("Review not found");
        }

        if (review.ReviewerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own reviews");
        }

        var storyId = review.StoryId;
        var result = await _reviewRepository.DeleteAsync(id);

        // Update story stats
        await UpdateStoryStats(storyId);

        return result;
    }

    public double CalculateOverallScore(CreateReviewRequest scores)
    {
        var weightedSum =
            scores.Clarity * Weights["clarity"] +
            scores.Ownership * Weights["ownership"] +
            scores.Impact * Weights["impact"] +
            scores.DecisionMaking * Weights["decisionMaking"] +
            scores.Communication * Weights["communication"] +
            scores.Reflection * Weights["reflection"];

        var totalWeight =
            Weights["clarity"] +
            Weights["ownership"] +
            Weights["impact"] +
            Weights["decisionMaking"] +
            Weights["communication"] +
            Weights["reflection"];

        if (scores.TechnicalDepth.HasValue)
        {
            weightedSum += scores.TechnicalDepth.Value * Weights["technicalDepth"];
            totalWeight += Weights["technicalDepth"];
        }

        return Math.Round(weightedSum / totalWeight, 2);
    }

    private double CalculateOverallScoreFromReview(Review review)
    {
        var weightedSum =
            review.Scores.Clarity * Weights["clarity"] +
            review.Scores.Ownership * Weights["ownership"] +
            review.Scores.Impact * Weights["impact"] +
            review.Scores.DecisionMaking * Weights["decisionMaking"] +
            review.Scores.Communication * Weights["communication"] +
            review.Scores.Reflection * Weights["reflection"];

        var totalWeight =
            Weights["clarity"] +
            Weights["ownership"] +
            Weights["impact"] +
            Weights["decisionMaking"] +
            Weights["communication"] +
            Weights["reflection"];

        if (review.Scores.TechnicalDepth.HasValue)
        {
            weightedSum += review.Scores.TechnicalDepth.Value * Weights["technicalDepth"];
            totalWeight += Weights["technicalDepth"];
        }

        return Math.Round(weightedSum / totalWeight, 2);
    }

    private static void ValidateScores(CreateReviewRequest request)
    {
        var scores = new[] { request.Clarity, request.Ownership, request.Impact,
                            request.DecisionMaking, request.Communication, request.Reflection };

        foreach (var score in scores)
        {
            if (score < 1 || score > 5)
            {
                throw new ArgumentException("All scores must be between 1 and 5");
            }
        }

        if (request.TechnicalDepth.HasValue && (request.TechnicalDepth < 1 || request.TechnicalDepth > 5))
        {
            throw new ArgumentException("Technical depth score must be between 1 and 5");
        }
    }

    private async Task UpdateStoryStats(string storyId)
    {
        var (averageScore, count, categoryAverages) = await _reviewRepository.GetStoryStatsAsync(storyId);

        var stats = new ReviewStats
        {
            TotalReviews = count,
            OverallScore = Math.Round(averageScore, 2),
            CategoryAverages = categoryAverages.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value, 2))
        };

        await _storyRepository.UpdateReviewStatsAsync(storyId, stats);
    }

    private async Task<ReviewDto> ToDto(Review review)
    {
        var reviewer = await _userRepository.GetByIdAsync(review.ReviewerId);
        return new ReviewDto(
            review.Id,
            review.StoryId,
            review.ReviewerId,
            reviewer?.DisplayName ?? "Unknown",
            reviewer?.AvatarUrl,
            review.Scores,
            review.OverallScore,
            review.Feedback,
            review.Strengths,
            review.Improvements,
            review.CreatedAt,
            review.UpdatedAt
        );
    }
}
