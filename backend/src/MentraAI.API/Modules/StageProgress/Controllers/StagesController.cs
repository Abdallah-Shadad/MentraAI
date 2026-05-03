// Modules/StageProgress/Controllers/StagesController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.StageProgress.Services;

namespace MentraAI.API.Modules.StageProgress.Controllers;

[ApiController]
[Route("api/v1/stages")]
[Authorize]
public class StagesController : ControllerBase
{
    private readonly IStageProgressService _stageService;

    public StagesController(IStageProgressService stageService)
    {
        _stageService = stageService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET /api/v1/stages
    [HttpGet]
    public async Task<IActionResult> GetAllStages()
    {
        var result = await _stageService.GetAllStagesAsync(GetUserId());
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/stages/current
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentStage()
    {
        var result = await _stageService.GetCurrentStageAsync(GetUserId());
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/stages/{stageProgressId}/enter
    [HttpPost("{stageProgressId:guid}/enter")]
    public async Task<IActionResult> EnterStage(Guid stageProgressId)
    {
        var result = await _stageService.EnterStageAsync(stageProgressId, GetUserId());
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/stages/{stageProgressId}/resources
    [HttpGet("{stageProgressId:guid}/resources")]
    public async Task<IActionResult> GetResources(Guid stageProgressId)
    {
        var result = await _stageService.GetResourcesAsync(stageProgressId, GetUserId());
        return Ok(ApiResponse<object>.Ok(result));
    }
}