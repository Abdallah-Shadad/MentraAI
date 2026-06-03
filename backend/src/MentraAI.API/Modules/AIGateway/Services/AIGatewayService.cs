using MentraAI.API.Common.Errors;
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
    // PREDICT CAREER
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

        var topTrack = aiRecommendation.RecommendedTracks.First();

        return new PredictionResult
        {
            PrimaryRoleName = topTrack.TrackName,
            PrimaryConfidence = (decimal)topTrack.FitScore / 100m,
            TopRolesJson = JsonSerializer.Serialize(aiRecommendation.RecommendedTracks.Select(t => new
            {
                name = t.TrackName,
                confidence = (decimal)t.FitScore / 100m
            }))
        };
    }

    // =====================================================================
    // GENERATE ROADMAP — Mode 1
    // =====================================================================
    public async Task<RoadmapGenerationResult> GenerateRoadmapAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string userBackground,
        List<string> currentSkills,
        CancellationToken ct = default)
    {
        var request = new
        {
            user_id = userId,
            career_track = careerTrack,
            weekly_hours = weeklyHours,
            is_stage_progression = false,
            current_stage = (object?)null,
            curriculum = (object?)null,
            current_stage_index = (int?)null,
            learner_progress = (object?)null
        };

        var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        _logger.LogInformation("AI Request => UserId: {UserId}, CareerTrack: {CareerTrack}", request.user_id, request.career_track);
        var response = await _http.PostAsJsonAsync("/api/v1/roadmap/", request, options, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}");
        }

        var rawJson = await response.Content.ReadAsStringAsync(ct);
        RoadmapAIResponse aiResponse = JsonSerializer.Deserialize<RoadmapAIResponse>(rawJson) ?? throw new AIValidationException("Empty response");

        RoadmapAIResponseValidator.Validate(aiResponse);
        var data = aiResponse.Roadmap!.Data!;

        return new RoadmapGenerationResult
        {
            RoadmapDataJson = rawJson,
            DifficultyLevel = data.DifficultyLevel,
            TotalWeeks = data.TotalWeeks,
            SkillGaps = data.SkillGaps,
            Stages = data.Curriculum!.Stages.Select(s => new RoadmapStage
            {
                AiStageId = s.Id,
                Name = s.Name,
                Topics = s.Topics,
                EstimatedWeeks = s.EstimatedWeeks
            }).ToList()
        };
    }

    // =====================================================================
    // GET STAGE RESOURCES — Mode 2
    // =====================================================================
    public async Task<StageResourcesResult> GetStageResourcesAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string aiStageId,
        string stageName,
        List<string> topics,
        List<string> learningObjectives,
        int estimatedWeeks,
        CancellationToken ct = default)
    {
        var request = new RoadmapAIRequest
        {
            UserId = userId,
            CareerTrack = careerTrack,
            WeeklyHours = weeklyHours,
            IsStageProgression = true,
            CurrentStage = new CurrentStagePayload
            {
                Id = aiStageId,
                Name = stageName,
                Topics = topics,
                LearningObjectives = learningObjectives,
                EstimatedWeeks = estimatedWeeks
            }
        };

        var response = await _http.PostAsJsonAsync("/api/v1/roadmap/", request, ct);

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
        // ── Pre-flight: validate mandatory AI contract fields ──────────────
        // Contract: user_id, career_track, stage_id, topics are ALL required.
        // Reject here with 422 (Unprocessable) instead of letting the AI
        // return a 400 that surfaces as a confusing 502 Bad Gateway.
        var contractViolations = new List<string>();
        if (string.IsNullOrWhiteSpace(userId)) contractViolations.Add("user_id is required.");
        if (string.IsNullOrWhiteSpace(careerTrack)) contractViolations.Add("career_track is required.");
        if (string.IsNullOrWhiteSpace(aiStageId)) contractViolations.Add("stage_id is required (AiStageId is empty — roadmap data may be incomplete).");
        if (topics == null || topics.Count == 0) contractViolations.Add("topics must contain at least one entry.");

        if (contractViolations.Count > 0)
        {
            var violations = string.Join(" | ", contractViolations);
            _logger.LogError("Quiz AI contract pre-flight failed: {Violations}", violations);
            throw new AppException(
                ErrorCodes.AI_CONTRACT_VIOLATION,
                $"Cannot call Quiz AI — mandatory fields missing: {violations}",
                422);
        }
        // ──────────────────────────────────────────────────────────────────

        var request = new QuizCreateAIRequest
        {
            UserId = userId,
            CareerTrack = careerTrack,
            AiStageId = aiStageId,
            StageName = stageName,
            DifficultyLevel = difficultyLevel ?? "beginner",
            Topics = topics! // pre-flight guard above guarantees non-null and non-empty

        };

        _logger.LogInformation(
            "Sending quiz generation request to AI. user_id={UserId}, career_track={CareerTrack}, stage_id={StageId}, topics_count={TopicsCount}, topics={Topics}",
            request.UserId, request.CareerTrack, request.AiStageId, request.Topics?.Count ?? 0,
            string.Join(", ", request.Topics ?? new()));

        var aiCallStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _http.PostAsJsonAsync("/api/v1/quiz/generate", request, ct);
        var rawBody = await response.Content.ReadAsStringAsync(ct);
        aiCallStopwatch.Stop();

        _logger.LogInformation(
            "Quiz generation endpoint returned. HTTP Status: {Status}, Time Taken: {ElapsedMs}ms, Payload Size: {Size} bytes",
            response.StatusCode, aiCallStopwatch.ElapsedMilliseconds, rawBody.Length);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("AI quiz error {Status}: {Body}", response.StatusCode, rawBody);
            throw new AIServiceException($"AI returned {(int)response.StatusCode}. Details: {rawBody}");
        }

        QuizAIResponse? aiResponse;
        try
        {
            aiResponse = JsonSerializer.Deserialize<QuizAIResponse>(rawBody);
            if (aiResponse != null)
            {
                _logger.LogInformation(
                    "AI reported time consumed: {TimeConsumed} seconds", 
                    aiResponse.TimeConsumed);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("AI quiz returned invalid JSON. Body: {Body}", rawBody);
            throw new AIValidationException($"AI returned invalid JSON: {ex.Message}");
        }

        try
        {
            QuizAIResponseValidator.Validate(aiResponse!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI quiz validation failed. Raw response: {Body}", rawBody);
            throw;
        }

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
    // GET ADAPTED ROADMAP (REMEDIATION) — Mode 3
    // =====================================================================
    public async Task<AdaptationResult> GetAdaptedRoadmapAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        List<string> learningObjectives,
        List<FailedQuestion> failedQuestions,
        decimal score,
        CancellationToken ct = default)
    {
        var request = new AdaptationAIRequest
        {
            UserId = userId,
            CareerTrack = careerTrack,
            StageId = aiStageId,
            StageName = stageName,
            Score = score,
            DifficultyLevel = difficultyLevel,
            LearningObjectives = learningObjectives,
            FailedQuestions = failedQuestions
        };

        var response = await _http.PostAsJsonAsync("/api/v1/quiz/adaptation_stage", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI adaptation error {Status}: {Body}", response.StatusCode, body);
            throw new AIServiceException($"Adaptation AI returned {(int)response.StatusCode}");
        }

        var rawBody = await response.Content.ReadAsStringAsync(ct);
        AdaptationAIResponse aiResponse = JsonSerializer.Deserialize<AdaptationAIResponse>(rawBody)!;
        AdaptationAIResponseValidator.Validate(aiResponse);

        var data = aiResponse.AdditionalResource!.Data!;

        return new AdaptationResult
        {
            RemediationResourcesJson = rawBody,
            Summary = data.Summary,
            StrugglingTopics = data.StrugglingTopics,
            Stages = data.Curriculum!.Stages.Select(s => new RoadmapStage
            {
                AiStageId = s.Id,
                Name = s.Name,
                EstimatedWeeks = s.EstimatedWeeks
            }).ToList()
        };
    }

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
    // GET TRACK RECOMMENDATIONS
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

        TrackRecommendAIResponseValidator.Validate(aiResponse!);
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