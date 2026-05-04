using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Validators;
using MentraAI.API.Modules.Users.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace MentraAI.API.Modules.AIGateway.Services;

public class AIGatewayService : IAIGatewayService
{
    private readonly HttpClient _http;
    private readonly ILogger<AIGatewayService> _logger;

    // HttpClient is injected by DI — BaseAddress and X-API-Key header
    // are configured in Program.cs via AddHttpClient, not here.
    public AIGatewayService(HttpClient http, ILogger<AIGatewayService> logger)
    {
        _http = http;
        _logger = logger;
    }


    // PREDICT CAREER  —  Phase 3 

    // fake implementation until AI team finalizes contract and we can implement the real HTTP call
    public async Task<PredictionResult> PredictCareerAsync(
    string userId,
    UserProfile profile,
    CancellationToken ct = default)
    {
        await Task.Delay(500, ct); // simulate AI latency with cancellation support

        return new PredictionResult
        {
            PrimaryRoleName = "Backend Engineer",
            PrimaryConfidence = 0.87m,
            TopRolesJson = """
        [
            {
                "name": "Backend Engineer",
                "confidence": 0.87
            },
            {
                "name": "Data Engineer",
                "confidence": 0.71
            }
        ]
        """
        };
    }

    public async Task<RoadmapGenerationResult> GenerateRoadmapAsync(
        string userId,
        string careerTrackSlug,
        int weeklyHours,
        string userBackground,
        List<string> currentSkills,
        CancellationToken ct = default)
    {
        var request = new RoadmapAIRequest
        {
            UserId = userId,
            CareerTrack = careerTrackSlug,
            WeeklyHours = weeklyHours,
            IsStageProgression = false,           // roadmap overview mode
            UserBackground = userBackground,
            CurrentSkills = currentSkills
        };

        // Use PostAsJsonAsync for better serialization and resilience
        var response = await _http.PostAsJsonAsync("/api/v1/roadmap", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI roadmap error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}");
        }

        var rawJson = await response.Content.ReadAsStringAsync(ct);
        RoadmapAIResponse aiResponse;
        try
        {
            aiResponse = JsonSerializer.Deserialize<RoadmapAIResponse>(rawJson)
                ?? throw new AIValidationException("AI returned empty roadmap response");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Malformed roadmap JSON from AI");
            throw new AIValidationException("AI returned malformed JSON");
        }

        RoadmapAIResponseValidator.Validate(aiResponse);

        var data = aiResponse.Roadmap!.Data!;
        var stages = data.Curriculum!.Stages;

        return new RoadmapGenerationResult
        {
            RoadmapDataJson = rawJson,  // store the entire AI response as-is (opaque)
            DifficultyLevel = data.DifficultyLevel,
            TotalWeeks = data.TotalWeeks,
            SkillGaps = data.SkillGaps,
            Stages = stages.Select(s => new RoadmapStage
            {
                AiStageId = s.Id,
                Name = s.Name,
                Topics = s.Topics,
                EstimatedWeeks = s.EstimatedWeeks
            }).ToList()
        };
    }

    // =====================================================================
    // GET STAGE RESOURCES  —  future Phase stub
    // =====================================================================
    public async Task<StageResourcesResult> GetStageResourcesAsync(
            string userId, string careerTrack, int weeklyHours,
            string aiStageId, int stageIndex, string roadmapDataJson, CancellationToken ct = default)
    {
        // 1. Parse existing roadmap data
        RoadmapData roadmapData;
        try
        {
            using var doc = JsonDocument.Parse(roadmapDataJson);
            var root = doc.RootElement;
            var dataEl = root.TryGetProperty("roadmap", out var rm) &&
                         rm.TryGetProperty("data", out var d) ? d : root;

            roadmapData = JsonSerializer.Deserialize<RoadmapData>(dataEl.GetRawText())
                ?? throw new AIValidationException("Stored roadmap data is invalid");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse stored roadmap JSON");
            throw new AIValidationException("Stored roadmap data is malformed");
        }

        var request = new RoadmapAIRequest
        {
            UserId = userId,
            CareerTrack = careerTrack,
            WeeklyHours = weeklyHours,
            IsStageProgression = true,
            CurrentStageIndex = stageIndex,
            DifficultyLevel = roadmapData.DifficultyLevel,
            SkillGaps = roadmapData.SkillGaps,
            Curriculum = JsonSerializer.SerializeToElement(roadmapData.Curriculum) // Opaque pass-through
        };

        var response = await _http.PostAsJsonAsync("/api/v1/roadmap", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI stage resources error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}");
        }

        var rawJson = await response.Content.ReadAsStringAsync(ct);

        // FIX Issue 4: Store the full raw JSON exactly as AI sent it to ensure StageProgressService can navigate it
        return new StageResourcesResult
        {
            ResourcesDataJson = rawJson
        };
    }

    // =====================================================================
    // GENERATE QUIZ  —  future Phase stub (Issue 5 Fix)
    // =====================================================================
    public Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        CancellationToken ct = default)
        => throw new NotImplementedException("GenerateQuizAsync — implemented in Phase 5"); // Stub restored[cite: 1]

    // =====================================================================
    // GET ADAPTED ROADMAP  —  future Phase stub
    // =====================================================================
    public Task<RoadmapGenerationResult> GetAdaptedRoadmapAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        string questionsDataJson,
        string userAnswersDataJson,
        decimal score,
        CancellationToken ct = default)
        => throw new NotImplementedException("GetAdaptedRoadmapAsync — implemented in Phase 6");

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================

    // Parses a JSON array string stored in UserProfile into List<string>.
    // Returns empty list on null input or parse failure — never throws.
    private static List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}