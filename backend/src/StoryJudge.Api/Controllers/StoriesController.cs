using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;

namespace StoryJudge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _storyService;

    public StoriesController(IStoryService storyService)
    {
        _storyService = storyService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<StoryListDto>>> GetPublicStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] StoryType? storyType = null,
        [FromQuery] string? tag = null,
        [FromQuery] string sortBy = "newest")
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _storyService.GetPublicStoriesAsync(page, pageSize, storyType, tag, sortBy);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<PaginatedResponse<StoryListDto>>> GetMyStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _storyService.GetMyStoriesAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StoryDto>> GetById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var story = await _storyService.GetByIdAsync(id, userId);

        if (story == null)
        {
            return NotFound();
        }

        return Ok(story);
    }

    [HttpGet("shared/{shareToken}")]
    public async Task<ActionResult<StoryDto>> GetByShareToken(string shareToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var story = await _storyService.GetByShareTokenAsync(shareToken, userId);

        if (story == null)
        {
            return NotFound();
        }

        return Ok(story);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<StoryDto>> Create([FromBody] CreateStoryRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required");
        }

        var story = await _storyService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = story.Id }, story);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<StoryDto>> Update(string id, [FromBody] UpdateStoryRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var story = await _storyService.UpdateAsync(id, request, userId);
        return Ok(story);
    }

    [HttpPost("{id}/publish")]
    [Authorize]
    public async Task<ActionResult<StoryDto>> Publish(string id, [FromBody] PublishStoryRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var story = await _storyService.PublishAsync(id, request, userId);
        return Ok(story);
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

        await _storyService.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPatch("{id}/coverage")]
    [Authorize]
    public async Task<ActionResult> UpdateCoverage(string id, [FromBody] UpdateCoverageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _storyService.UpdateCoverageOverrideAsync(id, request, userId);
        return NoContent();
    }
}
