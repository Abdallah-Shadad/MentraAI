using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.Roadmaps.Repositories;
using MentraAI.API.Modules.StageProgress.DTOs.Responses;
using MentraAI.API.Modules.StageProgress.Models;
using MentraAI.API.Modules.StageProgress.Repositories;
using MentraAI.API.Modules.Users.Services;
using System.Text.Json;

namespace MentraAI.API.Modules.StageProgress.Services;

public class StageProgressService : IStageProgressService
{
    private readonly IStageProgressRepository _stageRepo;
    private readonly IRoadmapRepository _roadmapRepo;
    private readonly ICareerTrackRepository _trackRepo;
    private readonly IAIGatewayService _aiGateway;
    private readonly IUserService _userService;

    public StageProgressService(
        IStageProgressRepository stageRepo,
        IRoadmapRepository roadmapRepo,
        ICareerTrackRepository trackRepo,
        IAIGatewayService aiGateway,
        IUserService userService)
    {
        _stageRepo = stageRepo;
        _roadmapRepo = roadmapRepo;
        _trackRepo = trackRepo;
        _aiGateway = aiGateway;
        _userService = userService;
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
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "No active stage found.", 404);

        // FIX Issue: Implement HasPendingQuizAsync in StageProgressRepository to check for pending quizzes
        var hasPendingQuiz = await _stageRepo.HasPendingQuizAsync(activeStage.Id);

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

        if (stage.Status == "LOCKED")
            throw new AppException(ErrorCodes.STAGE_LOCKED,
                "This stage is not unlocked yet.", 422);

        // Cache hit
        if (stage.ResourcesDataJson != null)
            return BuildResourcesResponse(stage);

        // FIX: Use safe access to profile data. 
        // Since WeeklyHours is gone, we use a default or derive it from YearsCode.
        var profileResponse = await _userService.GetProfileAsync(userId);

        // Example logic: if the user has 0-1 years of experience, assume 10 hours/week, 
        // otherwise assume 15. Adjust this business logic as needed.
        int weeklyHours = (profileResponse.YearsCode ?? 0) < 1 ? 10 : 15;

        // Parse curriculum details for current stage from RoadmapDataJson
        List<string> topics = new();
        List<string> learningObjectives = new();
        int estimatedWeeks = 2;

        try
        {
            using var doc = JsonDocument.Parse(roadmap.RoadmapDataJson);
            var root = doc.RootElement;
            var dataEl = root.TryGetProperty("roadmap", out var rm) &&
                         rm.TryGetProperty("data", out var d) ? d : root;

            if (dataEl.TryGetProperty("curriculum", out var curr) &&
                curr.TryGetProperty("stages", out var stagesEl) &&
                stagesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var stageEl in stagesEl.EnumerateArray())
                {
                    if (stageEl.TryGetProperty("id", out var idEl) && idEl.GetString() == stage.AiStageId)
                    {
                        if (stageEl.TryGetProperty("topics", out var topicsEl) && topicsEl.ValueKind == JsonValueKind.Array)
                        {
                            topics = topicsEl.EnumerateArray().Select(t => t.GetString() ?? "").ToList();
                        }
                        if (stageEl.TryGetProperty("learning_objectives", out var objectivesEl) && objectivesEl.ValueKind == JsonValueKind.Array)
                        {
                            learningObjectives = objectivesEl.EnumerateArray().Select(o => o.GetString() ?? "").ToList();
                        }
                        if (stageEl.TryGetProperty("estimated_weeks", out var weeksEl))
                        {
                            estimatedWeeks = weeksEl.GetInt32();
                        }
                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Silently fall back to defaults
        }

        // Cache miss — call AI for resources
        var result = await _aiGateway.GetStageResourcesAsync(
            userId: userId,
            careerTrack: userTrack.CareerTrack.Name,
            weeklyHours: weeklyHours,
            aiStageId: stage.AiStageId,
            stageName: stage.StageName,
            topics: topics,
            learningObjectives: learningObjectives,
            estimatedWeeks: estimatedWeeks);

        // Cache the result
        stage.ResourcesDataJson = result.ResourcesDataJson;
        await _stageRepo.UpdateAsync(stage);

        return BuildResourcesResponse(stage);
    }

    // ── GET /stages/{stageProgressId}/resources ──────────────────────────────
    public async Task<StageResourcesResponse> GetResourcesAsync(Guid stageProgressId, string userId)
    {
        var (stage, _, _) = await ValidateStageOwnershipAsync(stageProgressId, userId);

        // FIX Issue: Reject only LOCKED stages. Allow ACTIVE and COMPLETED.
        if (stage.Status == "LOCKED")
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

        if (!string.IsNullOrWhiteSpace(stage.ResourcesDataJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(stage.ResourcesDataJson);
                var root = doc.RootElement;

                // 1. Check if it's an Adaptation Response (contains Additional_Resource)
                if (root.TryGetProperty("Additional_Resource", out var addRes) &&
                    addRes.TryGetProperty("data", out var addData) &&
                    addData.TryGetProperty("curriculum", out var addCurr) &&
                    addCurr.TryGetProperty("stages", out var addStages) &&
                    addStages.ValueKind == JsonValueKind.Array)
                {
                    var targetStageEl = addStages.EnumerateArray()
                        .FirstOrDefault(s => (s.TryGetProperty("id", out var idEl) && idEl.GetString() == stage.AiStageId) 
                                             || (s.TryGetProperty("adapted", out var adaptEl) && adaptEl.GetBoolean()));
                    
                    if (targetStageEl.ValueKind == JsonValueKind.Object &&
                        targetStageEl.TryGetProperty("resources", out var resourcesEl) &&
                        resourcesEl.ValueKind == JsonValueKind.Array)
                    {
                        var remResources = JsonSerializer.Deserialize<List<RemediationResource>>(resourcesEl.GetRawText())
                                           ?? new List<RemediationResource>();
                        
                        foreach (var res in remResources)
                        {
                            var type = res.ResourceType.ToLowerInvariant();
                            if (type.Contains("video"))
                            {
                                resources.Videos.Add(new VideoResource
                                {
                                    Title = res.Title,
                                    Url = res.Url,
                                    DurationMinutes = res.DurationMin
                                });
                            }
                            else if (type.Contains("article") || type.Contains("blog") || type.Contains("read"))
                            {
                                resources.Articles.Add(new ArticleResource
                                {
                                    Title = res.Title,
                                    Url = res.Url,
                                    EstimatedMinutes = res.DurationMin
                                });
                            }
                            else
                            {
                                resources.Documentation.Add(new DocumentationResource
                                {
                                    Title = res.Title,
                                    Url = res.Url
                                });
                            }
                        }
                    }
                }
                // 2. Check if it's a Standard Response (contains roadmap -> data)
                else if (root.TryGetProperty("roadmap", out var roadmap) &&
                         roadmap.TryGetProperty("data", out var data))
                {
                    resources = JsonSerializer.Deserialize<StageResources>(data.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new StageResources();
                }
            }
            catch (Exception)
            {
                // Silently return empty resources instead of crashing.
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