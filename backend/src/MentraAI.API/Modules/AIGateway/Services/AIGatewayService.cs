using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Validators;
using MentraAI.API.Modules.Users.Models;
using System.Text;
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
        await Task.Delay(500); // simulate AI latency

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
        List<string> currentSkills)
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

       
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/api/v1/roadmap", content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("AI roadmap error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}");
        }

        var rawJson = await response.Content.ReadAsStringAsync();
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
    // Modules/AIGateway/Services/AIGatewayService.cs
    public async Task<StageResourcesResult> GetStageResourcesAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string aiStageId,
        int stageIndex,
        string roadmapDataJson)
    {
        // Parse stored roadmap data to extract curriculum + difficulty + skill_gaps
        // These are passed back to AI opaquely — backend never interprets them
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
            Curriculum = JsonSerializer.SerializeToElement(roadmapData.Curriculum)
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/api/v1/roadmap", content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("AI stage resources error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}");
        }

        var rawJson = await response.Content.ReadAsStringAsync();
        RoadmapAIResponse aiResponse;
        try
        {
            aiResponse = JsonSerializer.Deserialize<RoadmapAIResponse>(rawJson)
                ?? throw new AIValidationException("AI returned empty stage resources response");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Malformed stage resources JSON from AI");
            throw new AIValidationException("AI returned malformed JSON");
        }

        // Validate signal only — data structure for stage resources is opaque
        if (aiResponse.Signal != "201_Created")
            throw new AIValidationException($"Unexpected AI signal: {aiResponse.Signal}");

        if (aiResponse.Roadmap?.Data is null)
            throw new AIValidationException("AI stage resources response has no data");

        // Store the entire data block as-is — frontend consumes it directly
        var dataJson = JsonSerializer.Serialize(aiResponse.Roadmap.Data);

        return new StageResourcesResult
        {
            ResourcesDataJson = dataJson
        };
    }

    // =====================================================================
    // GENERATE QUIZ  —  future Phase stub
    // =====================================================================
    public Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        CancellationToken ct = default)
        => throw new NotImplementedException("GenerateQuizAsync — implemented in Phase 5");

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