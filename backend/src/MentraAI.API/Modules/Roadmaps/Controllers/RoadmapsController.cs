using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.Roadmaps.Services;

namespace MentraAI.API.Modules.Roadmaps.Controllers;

[ApiController]
[Route("api/v1/roadmaps")]
[Authorize]
public class RoadmapsController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;

    public RoadmapsController(IRoadmapService roadmapService)
    {
        _roadmapService = roadmapService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // POST /api/v1/roadmaps/generate
    // Empty body — backend uses authenticated user's active UserTrack automatically
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateRoadmap()
    {
        var result = await _roadmapService.GenerateRoadmapAsync(GetUserId());
        return StatusCode(201, ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/roadmaps/current
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRoadmap()
    {
        var result = await _roadmapService.GetCurrentRoadmapAsync(GetUserId());
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/roadmaps/history
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _roadmapService.GetHistoryAsync(GetUserId());
        return Ok(ApiResponse<object>.Ok(result));
    }
}