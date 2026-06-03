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
using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.StageProgress.Services;

public class StageProgressService : IStageProgressService
{
    private readonly IStageProgressRepository _stageRepo;
    private readonly IRoadmapRepository _roadmapRepo;
    private readonly ICareerTrackRepository _trackRepo;
    private readonly IAIGatewayService _aiGateway;
    private readonly IUserService _userService;
    private readonly ILogger<StageProgressService> _logger;

    public StageProgressService(
        IStageProgressRepository stageRepo,
        IRoadmapRepository roadmapRepo,
        ICareerTrackRepository trackRepo,
        IAIGatewayService aiGateway,
        IUserService userService,
        ILogger<StageProgressService> logger)
    {
        _stageRepo = stageRepo;
        _roadmapRepo = roadmapRepo;
        _trackRepo = trackRepo;
        _aiGateway = aiGateway;
        _userService = userService;
        _logger = logger;
    }

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

    public async Task<CurrentStageResponse> GetCurrentStageAsync(string userId)
    {
        var roadmap = await GetActiveRoadmapForUserAsync(userId);
        var activeStage = await _stageRepo.GetActiveStageAsync(roadmap.Id)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "No active stage found.", 404);

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

    public async Task<StageResourcesResponse> EnterStageAsync(Guid stageProgressId, string userId)
    {
        var (stage, userTrack, roadmap) = await ValidateStageOwnershipAsync(stageProgressId, userId);

        if (stage.Status == "LOCKED")
            throw new AppException(ErrorCodes.STAGE_LOCKED, "This stage is not unlocked yet.", 422);

        if (stage.ResourcesDataJson != null)
            return BuildResourcesResponse(stage);

        var profileResponse = await _userService.GetProfileAsync(userId);
        //int weeklyHours = (profileResponse.YearsCode ?? 0) < 1 ? 10 : 15;
        int weeklyHours = 25;

        var stageInfo = ExtractStageFromRoadmapJson(roadmap.RoadmapDataJson, stage.StageIndex);

        _logger.LogInformation("Extracted Topics: {TopicsCount}, Objectives: {ObjCount}",
              stageInfo.Topics.Count, stageInfo.LearningObjectives.Count);

        if (!stageInfo.Topics.Any())
        {
            _logger.LogWarning("WARNING: Topics list is empty for stage {StageName}! AI might reject this.", stage.StageName);
            stageInfo.Topics.Add(stage.StageName);
        }
        // FIXED: Changed careerTrack to use Slug instead of Name to perfectly match AI requirements
        var result = await _aiGateway.GetStageResourcesAsync(
            userId: userId,
            careerTrack: userTrack.CareerTrack.Name,
            weeklyHours: weeklyHours,
            aiStageId: stage.AiStageId,
            stageName: stage.StageName,
            topics: stageInfo.Topics,
            learningObjectives: stageInfo.LearningObjectives,
            estimatedWeeks: stageInfo.EstimatedWeeks);

        stage.ResourcesDataJson = result.ResourcesDataJson;
        await _stageRepo.UpdateAsync(stage);

        return BuildResourcesResponse(stage);
    }

    public async Task<StageResourcesResponse> GetResourcesAsync(Guid stageProgressId, string userId)
    {
        var (stage, _, _) = await ValidateStageOwnershipAsync(stageProgressId, userId);

        if (stage.Status == "LOCKED")
            throw new AppException(ErrorCodes.STAGE_LOCKED, "This stage is not unlocked yet.", 422);

        if (stage.ResourcesDataJson == null)
            throw new AppException(ErrorCodes.RESOURCES_NOT_FETCHED, "Resources not available yet. Enter the stage first.", 422);

        return BuildResourcesResponse(stage);
    }

    private async Task<Roadmap> GetActiveRoadmapForUserAsync(string userId)
    {
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active career track selected.", 422);

        return await _roadmapRepo.GetActiveRoadmapAsync(userTrack.Id)
            ?? throw new AppException(ErrorCodes.ROADMAP_NOT_FOUND, "No active roadmap found. Generate one first.", 404);
    }

    private async Task<(UserStageProgress stage, UserTrack userTrack, Roadmap roadmap)> ValidateStageOwnershipAsync(Guid stageProgressId, string userId)
    {
        var stage = await _stageRepo.GetByIdAsync(stageProgressId)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active career track selected.", 422);

        if (userTrack.CareerTrack is null)
            throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "Active track details are missing.", 422);

        var roadmap = await _roadmapRepo.GetActiveRoadmapAsync(userTrack.Id)
            ?? throw new AppException(ErrorCodes.ROADMAP_NOT_FOUND, "No active roadmap found.", 404);

        if (stage.RoadmapId != roadmap.Id)
            throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        return (stage, userTrack, roadmap);
    }

    private static StageInfo ExtractStageFromRoadmapJson(string roadmapDataJson, int stageIndex)
    {
        try
        {
            using var doc = JsonDocument.Parse(roadmapDataJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("roadmap", out var rm) &&
                rm.TryGetProperty("data", out var data) &&
                data.TryGetProperty("curriculum", out var curr) &&
                curr.TryGetProperty("stages", out var stagesEl))
            {
                var stages = stagesEl.EnumerateArray().ToList();
                if (stageIndex < stages.Count)
                {
                    var s = stages[stageIndex];
                    return new StageInfo
                    {
                        Topics = s.TryGetProperty("topics", out var topicsEl)
                            ? ParseJsonStringList(topicsEl)
                            : new List<string>(),
                        LearningObjectives = s.TryGetProperty("learning_objectives", out var loEl)
                            ? ParseJsonStringList(loEl)
                            : new List<string>(),
                        EstimatedWeeks = s.TryGetProperty("estimated_weeks", out var ew) ? ew.GetInt32() : 1
                    };
                }
            }
        }
        catch { }

        return new StageInfo(new(), new(), 1);
    }

    private static List<string> ParseJsonStringList(JsonElement element)
    {
        var list = new List<string>();
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var val = item.GetString();
                if (!string.IsNullOrWhiteSpace(val))
                    list.Add(val);
            }
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                var val = prop.Value.GetString();
                if (!string.IsNullOrWhiteSpace(val))
                    list.Add($"{prop.Name}: {val}");
                else if (!string.IsNullOrWhiteSpace(prop.Name))
                    list.Add(prop.Name);
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            var val = element.GetString();
            if (!string.IsNullOrWhiteSpace(val))
                list.Add(val);
        }
        return list;
    }

    private record StageInfo(List<string> Topics, List<string> LearningObjectives, int EstimatedWeeks)
    {
        public StageInfo() : this(new(), new(), 1) { }
    }

    // FIXED: Support for dual-format (Normal Mode 2 resources AND Mode 3 adaptation resources)
    private static StageResourcesResponse BuildResourcesResponse(UserStageProgress stage)
    {
        var resources = new StageResources();

        if (!string.IsNullOrWhiteSpace(stage.ResourcesDataJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(stage.ResourcesDataJson);
                var root = doc.RootElement;

                // Check if this JSON belongs to an Adaptation Engine (Mode 3) response
                if (root.TryGetProperty("Additional_Resource", out var additionalEl))
                {
                    if (additionalEl.TryGetProperty("data", out var adaptData) &&
                        adaptData.TryGetProperty("curriculum", out var adaptCurr) &&
                        adaptCurr.TryGetProperty("stages", out var adaptStages))
                    {
                        var targetStage = adaptStages.EnumerateArray().FirstOrDefault();
                        if (targetStage.ValueKind != JsonValueKind.Undefined && targetStage.TryGetProperty("resources", out var resList))
                        {
                            foreach (var item in resList.EnumerateArray())
                            {
                                var title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                                var url = item.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";
                                var source = item.TryGetProperty("source", out var src) ? src.GetString() ?? "" : "";

                                if (source.ToLower() == "youtube" || source.ToLower() == "video")
                                {
                                    resources.Videos.Add(new VideoResource { Title = title, Url = url });
                                }
                                else
                                {
                                    resources.Articles.Add(new ArticleResource { Title = title, Url = url });
                                }
                            }
                        }
                    }
                }
                else // Default Path: Mode 2 Normal Resources
                {
                    var raw = JsonSerializer.Deserialize<ResourcesRoot>(stage.ResourcesDataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var topicsResources = raw?.Roadmap?.Data?.StageResources?.TopicsResources ?? new List<TopicResources>();

                    foreach (var topic in topicsResources)
                    {
                        if (topic.Videos != null)
                            resources.Videos.AddRange(topic.Videos.Select(v => new VideoResource { Title = v.Title, Url = v.Url }));

                        if (topic.Articles != null)
                            resources.Articles.AddRange(topic.Articles.Select(a => new ArticleResource { Title = a.Title, Url = a.Url }));

                        if (topic.Documentation != null)
                            resources.Documentation.AddRange(topic.Documentation.Select(d => new DocumentationResource { Title = d.Title, Url = d.Url }));
                    }
                }
            }
            catch
            {
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

    private class ResourcesRoot
    {
        [JsonPropertyName("roadmap")] public ResourcesRoadmapPayload? Roadmap { get; set; }
    }

    private class ResourcesRoadmapPayload
    {
        [JsonPropertyName("data")] public ResourcesData? Data { get; set; }
    }

    private class ResourcesData
    {
        [JsonPropertyName("stage_resources")] public StageResourcesPayload? StageResources { get; set; }
    }

    private class StageResourcesPayload
    {
        [JsonPropertyName("topics_resources")] public List<TopicResources> TopicsResources { get; set; } = new();
    }

    private class TopicResources
    {
        [JsonPropertyName("topic_name")] public string TopicName { get; set; } = string.Empty;
        [JsonPropertyName("videos")] public List<ResourceItem>? Videos { get; set; }
        [JsonPropertyName("articles")] public List<ResourceItem>? Articles { get; set; }
        [JsonPropertyName("documentation")] public List<ResourceItem>? Documentation { get; set; }
    }

    private class ResourceItem
    {
        [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("quality_score")] public double QualityScore { get; set; }
    }
}