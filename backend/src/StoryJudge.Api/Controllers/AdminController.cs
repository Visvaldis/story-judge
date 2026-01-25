using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryJudge.Core.DTOs;
using StoryJudge.Core.Interfaces;

namespace StoryJudge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Moderator")]
public class AdminController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IUserRepository _userRepository;
    private readonly IReviewRepository _reviewRepository;

    public AdminController(
        IReportService reportService,
        IUserRepository userRepository,
        IReviewRepository reviewRepository)
    {
        _reportService = reportService;
        _userRepository = userRepository;
        _reviewRepository = reviewRepository;
    }

    [HttpGet("reports/pending")]
    public async Task<ActionResult<List<ReportDto>>> GetPendingReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var reports = await _reportService.GetPendingAsync(page, pageSize);
        return Ok(reports);
    }

    [HttpGet("reports")]
    public async Task<ActionResult<List<ReportDto>>> GetAllReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var reports = await _reportService.GetAllAsync(page, pageSize);
        return Ok(reports);
    }

    [HttpGet("reports/count")]
    public async Task<ActionResult<int>> GetPendingReportCount()
    {
        var count = await _reportService.GetPendingCountAsync();
        return Ok(new { count });
    }

    [HttpPost("reports/{id}/resolve")]
    public async Task<ActionResult<ReportDto>> ResolveReport(string id, [FromBody] ResolveReportRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var report = await _reportService.ResolveAsync(id, request, userId);
        return Ok(report);
    }

    [HttpPost("users/{userId}/ban")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> BanUser(string userId)
    {
        var result = await _userRepository.SetBannedAsync(userId, true);
        if (!result)
        {
            return NotFound("User not found");
        }
        return NoContent();
    }

    [HttpPost("users/{userId}/unban")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> UnbanUser(string userId)
    {
        var result = await _userRepository.SetBannedAsync(userId, false);
        if (!result)
        {
            return NotFound("User not found");
        }
        return NoContent();
    }

    [HttpPost("reviews/{reviewId}/hide")]
    public async Task<ActionResult> HideReview(string reviewId)
    {
        var result = await _reviewRepository.SetHiddenAsync(reviewId, true);
        if (!result)
        {
            return NotFound("Review not found");
        }
        return NoContent();
    }

    [HttpPost("reviews/{reviewId}/unhide")]
    public async Task<ActionResult> UnhideReview(string reviewId)
    {
        var result = await _reviewRepository.SetHiddenAsync(reviewId, false);
        if (!result)
        {
            return NotFound("Review not found");
        }
        return NoContent();
    }
}
