using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Validators;
using MentraAI.API.Modules.Chat.DTOs.Requests;
using MentraAI.API.Modules.Users.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.Services;

public class AIGatewayService : IAIGatewayService
{
    private readonly HttpClient _http;
    private readonly ILogger<AIGatewayService> _logger;

    public AIGatewayService(HttpClient http, ILogger<AIGatewayService> logger)
    {
        _http = http;
        _logger = logger;
    }

    // =====================================================================
    // PREDICT CAREER  —  Phase 3
    // =====================================================================
    public async Task<PredictionResult> PredictCareerAsync(
        string userId,
        UserProfile profile,
        CancellationToken ct = default)
    {
        var trackProfile = new TrackRecommendProfile
        {
            Age = profile.Age,
            EdLevel = profile.EdLevel,
            YearsCode = profile.YearsCode,
            WorkExp = profile.WorkExp,
            Employment = profile.Employment,
            RemoteWork = profile.RemoteWork,
            Industry = profile.Industry,
            OrgSize = profile.OrgSize,
            AISelect = profile.AISelect,
            CurrentSkills = ParseJsonArray(profile.CurrentSkillsJson),
            FutureSkills = ParseJsonArray(profile.FutureSkillsJson)
        };

        var aiRecommendation = await GetTrackRecommendationsAsync(userId, trackProfile, ct);

        // enforce that we have at least one recommendation (the validator should ensure this)
        var topTrack = aiRecommendation.RecommendedTracks.First();

        return new PredictionResult
        {
            //take the top recommendation as the primary one
            PrimaryRoleName = topTrack.TrackName,

            // confidence is normalized to 0-1 range for frontend display, assuming AI returns 0-100
            PrimaryConfidence = (decimal)topTrack.FitScore / 100m,

            // sends the full list of recommended tracks and their confidence for more detailed frontend display (e.g. a ranked list)
            TopRolesJson = JsonSerializer.Serialize(aiRecommendation.RecommendedTracks.Select(t => new
            {
                name = t.TrackName,
                confidence = (decimal)t.FitScore / 100m
            }))
        };
    }

    // =====================================================================
    // GENERATE ROADMAP
    // =====================================================================
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
            IsStageProgression = false,
            UserBackground = userBackground,
            CurrentSkills = currentSkills
        };

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
            RoadmapDataJson = rawJson,
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
    // GET STAGE RESOURCES
    // =====================================================================
    public async Task<StageResourcesResult> GetStageResourcesAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string aiStageId,
        int stageIndex,
        string roadmapDataJson,
        CancellationToken ct = default)
    {
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

        var response = await _http.PostAsJsonAsync("/api/v1/roadmap", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI stage resources error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}");
        }

        var rawJson = await response.Content.ReadAsStringAsync(ct);
        return new StageResourcesResult { ResourcesDataJson = rawJson };
    }

    // =====================================================================
    // GENERATE QUIZ
    // =====================================================================
    public async Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        List<string> topics,
        CancellationToken ct = default)
    {
        var request = new QuizCreateAIRequest
        {
            UserId = userId,
            CareerTrack = careerTrack,
            AiStageId = aiStageId,
            StageName = stageName,
            DifficultyLevel = difficultyLevel,
            Topics = topics
        };

        var response = await _http.PostAsJsonAsync("/api/v1/quiz/generate", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI quiz error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}");
        }

        var rawBody = await response.Content.ReadAsStringAsync(ct);

        QuizAIResponse? aiResponse;
        try
        {
            aiResponse = JsonSerializer.Deserialize<QuizAIResponse>(rawBody);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("AI quiz returned invalid JSON. Body: {Body}", rawBody);
            throw new AIValidationException($"AI returned invalid JSON: {ex.Message}");
        }

        QuizAIResponseValidator.Validate(aiResponse!);

        var questionsDataJson = JsonSerializer.Serialize(aiResponse!.Quiz!.Questions);

        var displayQuestions = aiResponse.Quiz.Questions
            .Select(q => new QuizQuestionDisplay
            {
                Id = q.QuestionId,
                Text = q.QuestionText,
                Choices = q.Choices.Select(c => new QuizChoiceDisplay
                {
                    Label = c.Label,
                    Text = c.Text
                }).ToList()
            })
            .ToList();

        _logger.LogInformation(
            "Quiz generated for user {UserId}, stage {StageId}, {Count} questions",
            userId, aiStageId, displayQuestions.Count);

        return new QuizGenerationResult
        {
            QuestionsDataJson = questionsDataJson,
            TotalQuestions = displayQuestions.Count,
            PassingScore = aiResponse.Quiz.PassingScore,
            TimeLimitMinutes = aiResponse.Quiz.TimeLimitMinutes,
            Questions = displayQuestions
        };
    }

    // =====================================================================
    // GET ADAPTED ROADMAP  —  Phase 6 stub
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
    // STREAM CHAT  
    // =====================================================================
    public async Task StreamChatAsync(
        ChatAIRequest request,
        HttpResponse httpResponse,
        CancellationToken ct = default)
    {
        var aiRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/chat/")
        {
            Content = JsonContent.Create(request)
        };

        using var aiResponse = await _http.SendAsync(
            aiRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!aiResponse.IsSuccessStatusCode)
        {
            var body = await aiResponse.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI chat error {Status}: {Body}", aiResponse.StatusCode, body);
            throw new AIServiceException($"Chat AI returned {(int)aiResponse.StatusCode}");
        }

        httpResponse.Headers["Content-Type"] = "text/event-stream";
        httpResponse.Headers["Cache-Control"] = "no-cache";
        httpResponse.Headers["X-Accel-Buffering"] = "no";

        using var stream = await aiResponse.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        try
        {
            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) break;

                await httpResponse.WriteAsync(line + "\n\n", ct);
                await httpResponse.Body.FlushAsync(ct);
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException || ex is IOException)
        {
            _logger.LogInformation("Streaming cancelled or disconnected for AI Chat.");
        }
    }

    // =====================================================================
    // DELETE CHAT MEMORY  
    // =====================================================================
    public async Task DeleteChatMemoryAsync(
        string userId,
        string conversationId,
        CancellationToken ct = default)
    {
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/chat/memory/")
        {
            Content = JsonContent.Create(new
            {
                user_id = userId,
                conversation_id = conversationId
            })
        };

        var response = await _http.SendAsync(deleteRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "AI memory delete returned {Status}: {Body}. Continuing anyway.",
                response.StatusCode, body);
        }
    }

    // =====================================================================
    // CHECK AI CHAT HEALTH
    // =====================================================================
    public async Task<bool> CheckChatHealthAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/v1/chat/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Chat AI health check failed or AI server is unreachable.");
            return false;
        }
    }

    // =====================================================================
    // GET TRACK RECOMMENDATIONS — Fix: Aligned with strict direct mapping
    // =====================================================================
    public async Task<TrackRecommendationResult> GetTrackRecommendationsAsync(
        string userId,
        TrackRecommendProfile profile,
        CancellationToken ct = default)
    {
        var request = new TrackRecommendAIRequest
        {
            UserId = userId,
            Profile = profile
        };

        var response = await _http.PostAsJsonAsync("/api/v1/tracks/recommend", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "Track recommender error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException(
                $"Track recommender returned {(int)response.StatusCode}");
        }

        var rawBody = await response.Content.ReadAsStringAsync(ct);

        TrackRecommendAIResponse? aiResponse;
        try
        {
            aiResponse = JsonSerializer.Deserialize<TrackRecommendAIResponse>(rawBody);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                "Track recommender returned invalid JSON. Body: {Body}", rawBody);
            throw new AIValidationException($"AI returned invalid JSON: {ex.Message}");
        }

        // Run the synchronized validator
        TrackRecommendAIResponseValidator.Validate(aiResponse!);

        // FIXED: Deep nested path mapping to accurately locate the matching data layer
        var output = aiResponse!.Recommendations!.Data!.Recommendations!;

        return new TrackRecommendationResult
        {
            UserSummary = output.UserSummary,
            PrimaryRecommendation = output.PrimaryRecommendation,
            ProfileCompleteness = output.ProfileCompleteness,
            MissingInfoSuggestions = output.MissingInfoSuggestions ?? new List<string>(),
            RecommendedTracks = output.RecommendedTracks
                .Select(t => new TrackMatch
                {
                    TrackName = t.TrackName,
                    FitScore = t.FitScore,
                    Reasoning = t.Reasoning,
                    SkillOverlap = t.SkillOverlap,
                    SkillsToLearn = t.SkillsToLearn,
                    EstimatedTransitionWeeks = t.EstimatedTransitionWeeks
                })
                .ToList()
        };
    }

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================
    private static List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
        catch { return new List<string>(); }
    }
}