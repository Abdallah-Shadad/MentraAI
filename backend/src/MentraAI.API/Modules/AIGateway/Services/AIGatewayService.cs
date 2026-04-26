using System.Text.Json;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Validators;
using MentraAI.API.Modules.Users.Models;

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
    public async Task<PredictionResult> PredictCareerAsync(
        string userId,
        UserProfile profile,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Calling AI prediction for user {UserId}", userId);

        // Build request — parse JSON arrays stored in profile columns
        // TEMP UNTIL AI TEAM FINALIZES PREDICT CONTRACT — we want to be flexible on what profile data we send, and how it's stored in DB
        var request = new PredictAIRequest
        {
            UserId = userId,
            Background = profile.Background ?? string.Empty,
            Skills = ParseJsonArray(profile.CurrentSkillsJson),
            Interests = ParseJsonArray(profile.InterestsJson),
            WeeklyHours = profile.WeeklyHours ?? 0,
            CareerGoals = profile.CareerGoals ?? string.Empty
        };

        // Send request
        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await _http.PostAsJsonAsync(
                "/api/v1/machine_model/predict", request, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI service unreachable for prediction");
            throw new AIServiceException("AI service is unavailable.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "AI prediction request timed out");
            throw;
        }

        // Read raw body before checking status — needed for logging on failure
        var rawBody = await httpResponse.Content.ReadAsStringAsync(ct);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "AI prediction failed. Status: {Status} Body: {Body}",
                (int)httpResponse.StatusCode, rawBody);
            throw new AIServiceException(
                $"AI service returned {(int)httpResponse.StatusCode}.");
        }

        // Deserialize
        PredictAIResponse? aiResponse;
        try
        {
            aiResponse = JsonSerializer.Deserialize<PredictAIResponse>(rawBody);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
                "AI prediction returned malformed JSON. Body: {Body}", rawBody);
            throw new AIValidationException("AI returned malformed JSON.");
        }

        if (aiResponse is null)
        {
            _logger.LogError("AI prediction returned null after deserialization");
            throw new AIValidationException("AI returned empty response.");
        }

        // Validate structure before returning to caller
        PredictAIResponseValidator.Validate(aiResponse);

        // Map AI response → InternalModel
        // Caller gets PredictionResult — never sees PredictAIResponse
        var topRolesJson = JsonSerializer.Serialize(
            aiResponse.TopRoles.Select(r => new { name = r.Name, confidence = r.Confidence }));

        _logger.LogInformation(
            "AI prediction succeeded for user {UserId}. PrimaryRole: {Role} ({Confidence:P0})",
            userId, aiResponse.PrimaryRole!.Name, aiResponse.PrimaryRole.Confidence);

        return new PredictionResult
        {
            PrimaryRoleName = aiResponse.PrimaryRole!.Name,
            PrimaryConfidence = aiResponse.PrimaryRole.Confidence,
            TopRolesJson = topRolesJson
        };
    }

    // =====================================================================
    // GENERATE ROADMAP  —  future Phase stub
    // =====================================================================
    public Task<RoadmapGenerationResult> GenerateRoadmapAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        UserProfile profile,
        CancellationToken ct = default)
        => throw new NotImplementedException("GenerateRoadmapAsync — implemented in Phase 3");

    // =====================================================================
    // GET STAGE RESOURCES  —  future Phase stub
    // =====================================================================
    public Task<StageResourcesResult> GetStageResourcesAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string aiStageId,
        int stageIndex,
        string roadmapDataJson,
        CancellationToken ct = default)
        => throw new NotImplementedException("GetStageResourcesAsync — implemented in Phase 4");

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