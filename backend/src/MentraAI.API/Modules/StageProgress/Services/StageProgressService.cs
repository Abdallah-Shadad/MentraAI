using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.Roadmaps.Repositories;
using MentraAI.API.Modules.StageProgress.DTOs.Responses;
using MentraAI.API.Modules.StageProgress.Models;
using MentraAI.API.Modules.StageProgress.Repositories;
using System.Text.Json;

namespace MentraAI.API.Modules.StageProgress.Services;

public class StageProgressService : IStageProgressService
{
    private readonly IStageProgressRepository _stageRepo;
    private readonly IRoadmapRepository _roadmapRepo;
    private readonly ICareerTrackRepository _trackRepo;
    private readonly IAIGatewayService _aiGateway;

    public StageProgressService(
        IStageProgressRepository stageRepo,
        IRoadmapRepository roadmapRepo,
        ICareerTrackRepository trackRepo,
        IAIGatewayService aiGateway)
    {
        _stageRepo = stageRepo;
        _roadmapRepo = roadmapRepo;
        _trackRepo = trackRepo;
        _aiGateway = aiGateway;
    }

    // ── GET /stages ─────────────────────────────────────────────────────────
    public async Task<StageListResponse> GetAllStagesAsync(string userId)
    {
        var roadmap = await GetActiveRoadmapForUserAsync(userId);
        var stages = await _stageRepo.GetByRoadmapIdAsync(roadmap.Id);

        return new StageListResponse
        {
            Stages = stages.Select(s => new StageItemResponse
            {
                StageProgressId = s.Id,
                StageName = s.StageName,
                StageIndex = s.StageIndex,
                Status = s.Status,
                HasResources = s.ResourcesDataJson != null
            }).ToList()
        };
    }

    // ── GET /stages/current ──────────────────────────────────────────────────
    public async Task<CurrentStageResponse> GetCurrentStageAsync(string userId)
    {
        var roadmap = await GetActiveRoadmapForUserAsync(userId);
        var activeStage = await _stageRepo.GetActiveStageAsync(roadmap.Id)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND,
                "No active stage found. All stages may be completed.", 404);

        // Check if a pending (not yet submitted) quiz exists for this stage
        var hasPendingQuiz = await _roadmapRepo.HasPendingQuizAsync(activeStage.Id);

        return new CurrentStageResponse
        {
            StageProgressId = activeStage.Id,
            StageName = activeStage.StageName,
            StageIndex = activeStage.StageIndex,
            Status = activeStage.Status,
            HasResources = activeStage.ResourcesDataJson != null,
            HasPendingQuiz = hasPendingQuiz
        };
    }

    // ── POST /stages/{stageProgressId}/enter ────────────────────────────────
    public async Task<StageResourcesResponse> EnterStageAsync(Guid stageProgressId, string userId)
    {
        var (stage, userTrack, roadmap) = await ValidateStageOwnershipAsync(stageProgressId, userId);

        // Verify stage is ACTIVE — reject LOCKED or COMPLETED
        if (stage.Status != "ACTIVE")
            throw new AppException(ErrorCodes.STAGE_LOCKED,
                "This stage is not unlocked yet.", 422);

        // Cache hit — return immediately without calling AI
        if (stage.ResourcesDataJson != null)
            return BuildResourcesResponse(stage);

        // Cache miss — call AI for resources
        var result = await _aiGateway.GetStageResourcesAsync(
            userId: userId,
            careerTrack: userTrack.CareerTrack.Name,
            weeklyHours: roadmap.UserTrack.WeeklyHours ?? 10,
            aiStageId: stage.AiStageId,
            stageIndex: stage.StageIndex,
            roadmapDataJson: roadmap.RoadmapDataJson);

        // Cache the result — subsequent calls to enter or resources use this
        stage.ResourcesDataJson = result.ResourcesDataJson;
        await _stageRepo.UpdateAsync(stage);

        return BuildResourcesResponse(stage);
    }

    // ── GET /stages/{stageProgressId}/resources ──────────────────────────────
    public async Task<StageResourcesResponse> GetResourcesAsync(Guid stageProgressId, string userId)
    {
        var (stage, _, _) = await ValidateStageOwnershipAsync(stageProgressId, userId);

        if (stage.Status != "ACTIVE")
            throw new AppException(ErrorCodes.STAGE_LOCKED,
                "This stage is not unlocked yet.", 422);

        if (stage.ResourcesDataJson == null)
            throw new AppException(ErrorCodes.RESOURCES_NOT_FETCHED,
                "Resources not available yet. Enter the stage first.", 422);

        return BuildResourcesResponse(stage);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Roadmap> GetActiveRoadmapForUserAsync(string userId)
    {
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK,
                "No active career track selected.", 422);

        return await _roadmapRepo.GetActiveRoadmapAsync(userTrack.Id)
            ?? throw new AppException(ErrorCodes.ROADMAP_NOT_FOUND,
                "No active roadmap found. Generate one first.", 404);
    }

    private async Task<(UserStageProgress stage, UserTrack userTrack, Roadmap roadmap)>
        ValidateStageOwnershipAsync(Guid stageProgressId, string userId)
    {
        var stage = await _stageRepo.GetByIdAsync(stageProgressId)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND,
                "Stage not found.", 404);

        // Verify this stage belongs to the user's active roadmap
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK,
                "No active career track selected.", 422);

        var roadmap = await _roadmapRepo.GetActiveRoadmapAsync(userTrack.Id)
            ?? throw new AppException(ErrorCodes.ROADMAP_NOT_FOUND,
                "No active roadmap found.", 404);

        if (stage.RoadmapId != roadmap.Id)
            throw new AppException(ErrorCodes.STAGE_NOT_FOUND,
                "Stage not found.", 404);

        return (stage, userTrack, roadmap);
    }

    private static StageResourcesResponse BuildResourcesResponse(UserStageProgress stage)
    {
        var resources = new StageResources();

        if (stage.ResourcesDataJson != null)
        {
            try
            {
                resources = JsonSerializer.Deserialize<StageResources>(stage.ResourcesDataJson)
                            ?? new StageResources();
            }
            catch
            {
                // Malformed cached JSON — return empty rather than crashing
                resources = new StageResources();
            }
        }

        return new StageResourcesResponse
        {
            StageProgressId = stage.Id,
            StageName = stage.StageName,
            StageIndex = stage.StageIndex,
            Status = stage.Status,
            Resources = resources
        };
    }
}