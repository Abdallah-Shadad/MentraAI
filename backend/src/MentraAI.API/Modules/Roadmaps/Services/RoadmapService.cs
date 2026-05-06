using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Roadmaps.DTOs.Responses;
using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.Roadmaps.Repositories;
using MentraAI.API.Modules.StageProgress.Models;
using MentraAI.API.Modules.StageProgress.Repositories;
using MentraAI.API.Modules.Users.Services;
using System.Text.Json;

namespace MentraAI.API.Modules.Roadmaps.Services;

public class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepo;
    private readonly ICareerTrackRepository _trackRepo;
    private readonly IUserService _userService;
    private readonly IStageProgressRepository _stageRepo;
    private readonly IAIGatewayService _aiGateway;

    public RoadmapService(
        IRoadmapRepository roadmapRepo,
        ICareerTrackRepository trackRepo,
        IUserService userService,
        IStageProgressRepository stageRepo,
        IAIGatewayService aiGateway)
    {
        _roadmapRepo = roadmapRepo;
        _trackRepo = trackRepo;
        _userService = userService;
        _stageRepo = stageRepo;
        _aiGateway = aiGateway;
    }

    public async Task<RoadmapResponse> GenerateRoadmapAsync(string userId)
    {
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
             ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active track.", 422);

        if (await _roadmapRepo.HasActiveRoadmapAsync(userTrack.Id))
            throw new AppException(ErrorCodes.ROADMAP_ALREADY_EXISTS, "An active roadmap already exists.", 409);

        var profile = await _userService.GetProfileAsync(userId);

        var result = await _aiGateway.GenerateRoadmapAsync(
            userId: userId,
            careerTrackSlug: userTrack.CareerTrack.Slug,
            weeklyHours: profile.WeeklyHours ?? 10,
            userBackground: profile.Background ?? string.Empty,
            currentSkills: profile.CurrentSkills ?? new List<string>());

        // Create new roadmap version without saving yet (we need the Id for stages, so we save both in one transaction at the end of this method)
        var roadmap = new Roadmap
        {
            UserTrackId = userTrack.Id,
            VersionNumber = 1,
            IsActive = true,
            TriggerType = "INITIAL",
            RoadmapDataJson = result.RoadmapDataJson,
            CreatedAt = DateTime.UtcNow
        };

        // Prepare stages for insertion 
        var stages = result.Stages.Select((s, i) => new UserStageProgress
        {
            Id = Guid.NewGuid(),
            StageIndex = i,
            AiStageId = s.AiStageId,
            StageName = s.Name,
            Status = i == 0 ? "ACTIVE" : "LOCKED",
            UnlockedAt = i == 0 ? DateTime.UtcNow : null,
            CompletedAt = null
        }).ToList();

        // Save both roadmap and stages in one transaction
        await _roadmapRepo.CreateWithStagesAsync(roadmap, stages);

        return BuildRoadmapResponse(roadmap, result.DifficultyLevel, result.TotalWeeks, result.SkillGaps, stages, result.Stages);
    }

    public async Task<RoadmapResponse> GetCurrentRoadmapAsync(string userId)
    {
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active track.", 422);

        var roadmap = await _roadmapRepo.GetActiveRoadmapAsync(userTrack.Id)
            ?? throw new AppException(ErrorCodes.ROADMAP_NOT_FOUND, "No active roadmap found.", 404);

        var stageRows = await _stageRepo.GetByRoadmapIdAsync(roadmap.Id);
        var (difficultyLevel, totalWeeks, skillGaps, aiStages) = ParseRoadmapDataJson(roadmap.RoadmapDataJson);

        return BuildRoadmapResponse(roadmap, difficultyLevel, totalWeeks, skillGaps, stageRows, aiStages);
    }

    public async Task<RoadmapHistoryResponse> GetHistoryAsync(string userId)
    {
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active track.", 422);

        var versions = await _roadmapRepo.GetAllVersionsAsync(userTrack.Id);

        return new RoadmapHistoryResponse
        {
            Versions = versions.Select(r => new RoadmapSummaryResponse
            {
                RoadmapId = r.Id,
                VersionNumber = r.VersionNumber,
                IsActive = r.IsActive,
                TriggerType = r.TriggerType,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    public async Task<Roadmap> AdaptRoadmapAsync(
        Guid stageProgressId,
        string questionsDataJson,
        string userAnswersDataJson,
        decimal score,
        string userId)
    {
        var stage = await _stageRepo.GetByIdAsync(stageProgressId)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active track.", 422);

        var currentRoadmap = await _roadmapRepo.GetActiveRoadmapAsync(userTrack.Id)
            ?? throw new AppException(ErrorCodes.ROADMAP_NOT_FOUND, "No active roadmap.", 404);

        var adaptResult = await _aiGateway.GetAdaptedRoadmapAsync(
            userId: userId,
            careerTrack: userTrack.CareerTrack.Slug,
            aiStageId: stage.AiStageId,
            stageName: stage.StageName,
            difficultyLevel: ExtractDifficultyLevel(currentRoadmap.RoadmapDataJson),
            questionsDataJson: questionsDataJson,
            userAnswersDataJson: userAnswersDataJson,
            score: score);

        // Prepare new version data
        var nextVersion = await _roadmapRepo.GetMaxVersionAsync(userTrack.Id) + 1;
        var newRoadmap = new Roadmap
        {
            UserTrackId = userTrack.Id,
            VersionNumber = nextVersion,
            IsActive = true,
            TriggerType = "ADAPTATION",
            RoadmapDataJson = adaptResult.RoadmapDataJson,
            CreatedAt = DateTime.UtcNow
        };

        var newStages = new List<UserStageProgress>();
        int failedIndex = stage.StageIndex;

        for (int i = 0; i < adaptResult.Stages.Count; i++)
        {
            var s = adaptResult.Stages[i];
            var status = i < failedIndex ? "COMPLETED"
                       : i == failedIndex ? "ACTIVE"
                       : "LOCKED";

            newStages.Add(new UserStageProgress
            {
                Id = Guid.NewGuid(),
                StageIndex = i,
                AiStageId = s.AiStageId,
                StageName = s.Name,
                Status = status,
                UnlockedAt = status == "ACTIVE" ? DateTime.UtcNow : null,
                CompletedAt = null
            });
        }

        // Delegate transaction and saving to repository
        return await _roadmapRepo.CreateNewVersionAsync(currentRoadmap, newRoadmap, newStages);
    }

    private static RoadmapResponse BuildRoadmapResponse(
        Roadmap roadmap,
        string difficultyLevel,
        int totalWeeks,
        List<string> skillGaps,
        List<UserStageProgress> stageRows,
        List<RoadmapStage> aiStages)
    {
        var stageItems = stageRows
            .OrderBy(r => r.StageIndex)
            .Select(row =>
            {
                var aiStage = aiStages.ElementAtOrDefault(row.StageIndex);
                return new StageProgressItem
                {
                    StageProgressId = row.Id,
                    StageName = row.StageName,
                    StageIndex = row.StageIndex,
                    EstimatedWeeks = aiStage?.EstimatedWeeks ?? 0,
                    Status = row.Status
                };
            }).ToList();

        return new RoadmapResponse
        {
            RoadmapId = roadmap.Id,
            VersionNumber = roadmap.VersionNumber,
            TriggerType = roadmap.TriggerType,
            DifficultyLevel = difficultyLevel,
            TotalWeeks = totalWeeks,
            SkillGaps = skillGaps,
            Stages = stageItems,
            CreatedAt = roadmap.CreatedAt
        };
    }

    private static (string, int, List<string>, List<RoadmapStage>) ParseRoadmapDataJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            JsonElement data = root.TryGetProperty("roadmap", out var rm) &&
                               rm.TryGetProperty("data", out var d) ? d : root;

            var difficulty = data.TryGetProperty("difficulty_level", out var dl) ? dl.GetString() ?? "" : "";
            var weeks = data.TryGetProperty("total_weeks", out var tw) ? tw.GetInt32() : 0;
            var skillGaps = data.TryGetProperty("skill_gaps", out var sg)
                ? sg.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                : new List<string>();

            var stages = new List<RoadmapStage>();
            if (data.TryGetProperty("curriculum", out var curr) &&
                curr.TryGetProperty("stages", out var stagesEl))
            {
                foreach (var s in stagesEl.EnumerateArray())
                {
                    stages.Add(new RoadmapStage
                    {
                        AiStageId = s.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
                        Name = s.TryGetProperty("name", out var nm) ? nm.GetString() ?? "" : "",
                        EstimatedWeeks = s.TryGetProperty("estimated_weeks", out var ew) ? ew.GetInt32() : 0,
                        Topics = s.TryGetProperty("topics", out var top)
                            ? top.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                            : new List<string>()
                    });
                }
            }
            return (difficulty, weeks, skillGaps, stages);
        }
        catch { return ("", 0, new(), new()); }
    }

    private static string ExtractDifficultyLevel(string roadmapDataJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(roadmapDataJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("roadmap", out var rm) &&
                rm.TryGetProperty("data", out var data) &&
                data.TryGetProperty("difficulty_level", out var dl))
                return dl.GetString() ?? "";
        }
        catch { }
        return "";
    }
}