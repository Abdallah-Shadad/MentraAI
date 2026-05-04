// Modules/Roadmaps/Services/RoadmapService.cs
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Data;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Roadmaps.DTOs.Responses;
using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.Roadmaps.Repositories;
using MentraAI.API.Modules.StageProgress.Models;
using MentraAI.API.Modules.StageProgress.Repositories;
using MentraAI.API.Modules.Users.Repositories;
using System.Text.Json;

namespace MentraAI.API.Modules.Roadmaps.Services;

public class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepo;
    private readonly ICareerTrackRepository _trackRepo;
    private readonly IUserRepository _userRepo;
    private readonly IStageProgressRepository _stageRepo;
    private readonly IAIGatewayService _aiGateway;
    private readonly AppDbContext _db;

    public RoadmapService(
        IRoadmapRepository roadmapRepo,
        ICareerTrackRepository trackRepo,
        IUserRepository userRepo,
        IStageProgressRepository stageRepo,
        IAIGatewayService aiGateway,
        AppDbContext db)
    {
        _roadmapRepo = roadmapRepo;
        _trackRepo = trackRepo;
        _userRepo = userRepo;
        _stageRepo = stageRepo;
        _aiGateway = aiGateway;
        _db = db;
    }

    // ── POST /roadmaps/generate ─────────────────────────────────────────────
    public async Task<RoadmapResponse> GenerateRoadmapAsync(string userId)
    {
        // Step 1: get user's active track
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK,
                "No active career track selected.", 422);

        // Step 2: check no active roadmap exists for this track
        if (await _roadmapRepo.HasActiveRoadmapAsync(userTrack.Id))
            throw new AppException(ErrorCodes.ROADMAP_ALREADY_EXISTS,
                "An active roadmap already exists for this career track.", 409);

        // Step 3: get user profile for AI request body
        var profile = await _userRepo.GetProfileByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "User profile not found.", 404);

        var currentSkills = string.IsNullOrWhiteSpace(profile.CurrentSkillsJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(profile.CurrentSkillsJson) ?? new();

        // Step 4: call AI — GenerateRoadmapAsync sends POST /api/v1/roadmap with is_stage_progression: false
        var result = await _aiGateway.GenerateRoadmapAsync(
            userId: userId,
            careerTrackSlug: userTrack.CareerTrack.Slug,
            weeklyHours: profile.WeeklyHours ?? 10,
            userBackground: profile.Background ?? string.Empty,
            currentSkills: currentSkills);

        // Step 5: save roadmap row (TriggerType: INITIAL, VersionNumber: 1)
        var roadmap = await _roadmapRepo.CreateAsync(new Roadmap
        {
            UserTrackId = userTrack.Id,
            VersionNumber = 1,
            IsActive = true,
            TriggerType = "INITIAL",
            RoadmapDataJson = result.RoadmapDataJson,
            CreatedAt = DateTime.UtcNow
        });

        // Step 6: create UserStageProgress rows — index 0 = ACTIVE, all others = LOCKED
        var stageProgressRows = new List<UserStageProgress>();
        for (int i = 0; i < result.Stages.Count; i++)
        {
            var s = result.Stages[i];
            var row = new UserStageProgress
            {
                Id = Guid.NewGuid(),
                RoadmapId = roadmap.Id,
                StageIndex = i,
                AiStageId = s.AiStageId,
                StageName = s.Name,
                Status = i == 0 ? "ACTIVE" : "LOCKED",
                UnlockedAt = i == 0 ? DateTime.UtcNow : null,
                CompletedAt = null
            };
            await _stageRepo.CreateAsync(row);
            stageProgressRows.Add(row);
        }

        // Step 7: return roadmap summary with backend stageProgressIds
        return BuildRoadmapResponse(roadmap, result.DifficultyLevel, result.TotalWeeks,
            result.SkillGaps, stageProgressRows, result.Stages);
    }

    // ── GET /roadmaps/current ───────────────────────────────────────────────
    public async Task<RoadmapResponse> GetCurrentRoadmapAsync(string userId)
    {
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK,
                "No active career track selected.", 422);

        var roadmap = await _roadmapRepo.GetActiveRoadmapAsync(userTrack.Id)
            ?? throw new AppException(ErrorCodes.ROADMAP_NOT_FOUND,
                "No active roadmap found. Generate one first.", 404);

        var stageRows = await _stageRepo.GetByRoadmapIdAsync(roadmap.Id);

        // Parse difficulty/totalWeeks/skillGaps from stored JSON (display only)
        var (difficultyLevel, totalWeeks, skillGaps, aiStages) = ParseRoadmapDataJson(roadmap.RoadmapDataJson);

        return BuildRoadmapResponse(roadmap, difficultyLevel, totalWeeks, skillGaps, stageRows, aiStages);
    }

    // ── GET /roadmaps/history ───────────────────────────────────────────────
    public async Task<RoadmapHistoryResponse> GetHistoryAsync(string userId)
    {
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK,
                "No active career track selected.", 422);

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

    // ── AdaptRoadmapAsync — called by QuizService on quiz fail ─────────────
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

        // AiStageId is passed back to AI for adaptation context — never used for any backend lookup
        var adaptResult = await _aiGateway.GetAdaptedRoadmapAsync(
            userId: userId,
            careerTrack: userTrack.CareerTrack.Slug,
            aiStageId: stage.AiStageId,
            stageName: stage.StageName,
            difficultyLevel: ExtractDifficultyLevel(currentRoadmap.RoadmapDataJson),
            questionsDataJson: questionsDataJson,
            userAnswersDataJson: userAnswersDataJson,
            score: score);

        // All DB changes in one transaction — deactivate old, insert new
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Deactivate current roadmap
            currentRoadmap.IsActive = false;
            await _roadmapRepo.UpdateAsync(currentRoadmap);

            // Create new roadmap version
            var nextVersion = await _roadmapRepo.GetMaxVersionAsync(userTrack.Id) + 1;
            var newRoadmap = await _roadmapRepo.CreateAsync(new Roadmap
            {
                UserTrackId = userTrack.Id,
                VersionNumber = nextVersion,
                IsActive = true,
                TriggerType = "ADAPTATION",
                RoadmapDataJson = adaptResult.RoadmapDataJson,
                CreatedAt = DateTime.UtcNow
            });

            int failedIndex = stage.StageIndex;

            // Create new UserStageProgress rows for new roadmap version
            // Stages before failed index: COMPLETED (CompletedAt = null — no fake timestamps)
            // Failed index stage: ACTIVE (reset)
            // Remaining stages: LOCKED
            for (int i = 0; i < adaptResult.Stages.Count; i++)
            {
                var s = adaptResult.Stages[i];
                var status = i < failedIndex ? "COMPLETED"
                           : i == failedIndex ? "ACTIVE"
                           : "LOCKED";

                await _stageRepo.CreateAsync(new UserStageProgress
                {
                    Id = Guid.NewGuid(),
                    RoadmapId = newRoadmap.Id,
                    StageIndex = i,
                    AiStageId = s.AiStageId,
                    StageName = s.Name,
                    Status = status,
                    UnlockedAt = status == "ACTIVE" ? DateTime.UtcNow : null,
                    CompletedAt = null  // never fabricate completion timestamps
                });
            }

            await tx.CommitAsync();
            return newRoadmap;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

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

            // Navigate into the nested roadmap.data structure
            var root = doc.RootElement;
            JsonElement data;

            if (root.TryGetProperty("roadmap", out var roadmapEl) &&
                roadmapEl.TryGetProperty("data", out data))
            {
                // response was stored from AI
            }
            else
            {
                data = root;
            }

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
        catch
        {
            return ("", 0, new(), new());
        }
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
        catch { /* ignore */ }
        return "";
    }
}