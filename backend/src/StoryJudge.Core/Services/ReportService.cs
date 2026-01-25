using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IReviewRepository _reviewRepository;

    public ReportService(
        IReportRepository reportRepository,
        IUserRepository userRepository,
        IStoryRepository storyRepository,
        IReviewRepository reviewRepository)
    {
        _reportRepository = reportRepository;
        _userRepository = userRepository;
        _storyRepository = storyRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<ReportDto?> GetByIdAsync(string id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null) return null;
        return await ToDto(report);
    }

    public async Task<List<ReportDto>> GetPendingAsync(int page, int pageSize)
    {
        var reports = await _reportRepository.GetByStatusAsync(ReportStatus.Pending, page, pageSize);
        var dtos = new List<ReportDto>();
        foreach (var report in reports)
        {
            dtos.Add(await ToDto(report));
        }
        return dtos;
    }

    public async Task<List<ReportDto>> GetAllAsync(int page, int pageSize)
    {
        var reports = await _reportRepository.GetAllAsync(page, pageSize);
        var dtos = new List<ReportDto>();
        foreach (var report in reports)
        {
            dtos.Add(await ToDto(report));
        }
        return dtos;
    }

    public async Task<ReportDto> CreateAsync(CreateReportRequest request, string reporterId)
    {
        // Validate target exists
        var targetExists = request.TargetType switch
        {
            ReportTargetType.Story => await _storyRepository.GetByIdAsync(request.TargetId) != null,
            ReportTargetType.Review => await _reviewRepository.GetByIdAsync(request.TargetId) != null,
            ReportTargetType.User => await _userRepository.GetByIdAsync(request.TargetId) != null,
            _ => false
        };

        if (!targetExists)
        {
            throw new KeyNotFoundException("Target not found");
        }

        var report = new Report
        {
            ReporterId = reporterId,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            Reason = request.Reason,
            Details = request.Details,
            Status = ReportStatus.Pending
        };

        await _reportRepository.CreateAsync(report);
        return await ToDto(report);
    }

    public async Task<ReportDto> ResolveAsync(string id, ResolveReportRequest request, string moderatorId)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        if (report == null)
        {
            throw new KeyNotFoundException("Report not found");
        }

        // Execute action
        switch (request.Action.ToLower())
        {
            case "hide_content":
                await HideContent(report);
                break;
            case "ban_user":
                await BanUser(report);
                break;
            case "dismiss":
                // No action needed
                break;
            default:
                throw new ArgumentException("Invalid action");
        }

        report.Status = request.Action.ToLower() == "dismiss" ? ReportStatus.Dismissed : ReportStatus.Resolved;
        report.ResolvedById = moderatorId;
        report.Resolution = request.Resolution;
        report.ResolvedAt = DateTime.UtcNow;

        await _reportRepository.UpdateAsync(report);
        return await ToDto(report);
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _reportRepository.GetPendingCountAsync();
    }

    private async Task HideContent(Report report)
    {
        switch (report.TargetType)
        {
            case ReportTargetType.Story:
                var story = await _storyRepository.GetByIdAsync(report.TargetId);
                if (story != null)
                {
                    story.Visibility = Visibility.Private;
                    await _storyRepository.UpdateAsync(story);
                }
                break;
            case ReportTargetType.Review:
                await _reviewRepository.SetHiddenAsync(report.TargetId, true);
                break;
        }
    }

    private async Task BanUser(Report report)
    {
        string? userId = null;

        switch (report.TargetType)
        {
            case ReportTargetType.User:
                userId = report.TargetId;
                break;
            case ReportTargetType.Story:
                var story = await _storyRepository.GetByIdAsync(report.TargetId);
                userId = story?.AuthorId;
                break;
            case ReportTargetType.Review:
                var review = await _reviewRepository.GetByIdAsync(report.TargetId);
                userId = review?.ReviewerId;
                break;
        }

        if (!string.IsNullOrEmpty(userId))
        {
            await _userRepository.SetBannedAsync(userId, true);
        }
    }

    private async Task<ReportDto> ToDto(Report report)
    {
        var reporter = await _userRepository.GetByIdAsync(report.ReporterId);
        var resolver = report.ResolvedById != null
            ? await _userRepository.GetByIdAsync(report.ResolvedById)
            : null;

        return new ReportDto(
            report.Id,
            report.ReporterId,
            reporter?.DisplayName ?? "Unknown",
            report.TargetType.ToString(),
            report.TargetId,
            report.Reason.ToString(),
            report.Details,
            report.Status.ToString(),
            report.ResolvedById,
            resolver?.DisplayName,
            report.Resolution,
            report.CreatedAt,
            report.ResolvedAt
        );
    }
}
