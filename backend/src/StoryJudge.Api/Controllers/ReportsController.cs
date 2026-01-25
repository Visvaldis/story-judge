using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryJudge.Core.DTOs;
using StoryJudge.Core.Interfaces;

namespace StoryJudge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReportDto>> Create([FromBody] CreateReportRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var report = await _reportService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Moderator")]
    public async Task<ActionResult<ReportDto>> GetById(string id)
    {
        var report = await _reportService.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound();
        }
        return Ok(report);
    }
}
