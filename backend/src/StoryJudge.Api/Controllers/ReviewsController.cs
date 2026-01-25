using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryJudge.Core.DTOs;
using StoryJudge.Core.Interfaces;

namespace StoryJudge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("story/{storyId}")]
    public async Task<ActionResult<List<ReviewDto>>> GetByStoryId(
        string storyId,
        [FromQuery] string? shareToken = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reviews = await _reviewService.GetByStoryIdAsync(storyId, userId, shareToken);
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetById(string id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }
        return Ok(review);
    }

    [HttpPost("story/{storyId}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> Create(string storyId, [FromBody] CreateReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var review = await _reviewService.CreateAsync(storyId, request, userId);
        return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> Update(string id, [FromBody] UpdateReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var review = await _reviewService.UpdateAsync(id, request, userId);
        return Ok(review);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _reviewService.DeleteAsync(id, userId);
        return NoContent();
    }
}
