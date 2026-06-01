using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;
using MentraAI.API.Modules.Quizzes.Models;
using MentraAI.API.Modules.Quizzes.Repositories;
using MentraAI.API.Modules.Roadmaps.Services;
using MentraAI.API.Modules.StageProgress.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.Quizzes.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepo;
    private readonly IStageProgressRepository _stageRepo;
    private readonly ICareerTrackRepository _trackRepo;
    private readonly IAIGatewayService _aiGateway;
    private readonly IQuizScoringService _scoring;
    private readonly IRoadmapService _roadmapService;
    private readonly IMapper _mapper;
    private readonly ILogger<QuizService> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QuizService(
        IQuizRepository quizRepo,
        IStageProgressRepository stageRepo,
        ICareerTrackRepository trackRepo,
        IAIGatewayService aiGateway,
        IQuizScoringService scoring,
        IRoadmapService roadmapService,
        IMapper mapper,
        ILogger<QuizService> logger)
    {
        _quizRepo = quizRepo;
        _stageRepo = stageRepo;
        _trackRepo = trackRepo;
        _aiGateway = aiGateway;
        _scoring = scoring;
        _roadmapService = roadmapService;
        _mapper = mapper;
        _logger = logger;
    }

    // =====================================================================
    // GENERATE NEW QUIZ ATTEMPT
    // =====================================================================
    public async Task<QuizResponse> GenerateQuizAsync(Guid stageProgressId, string userId)
    {
        var stage = await _stageRepo.GetByIdAsync(stageProgressId)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        if (stage.Status == "LOCKED")
            throw new AppException(ErrorCodes.STAGE_LOCKED, "Cannot take quiz for locked stage.", 422);

        // Check if a pending quiz already exists for this stage
        var hasPending = await _stageRepo.HasPendingQuizAsync(stageProgressId);
        if (hasPending)
            throw new AppException(ErrorCodes.QUIZ_PENDING_EXISTS, "A pending quiz already exists for this stage.", 409);

        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active track.", 422);

        var topics = ExtractTopicsFromRoadmapJson(stage.Roadmap.RoadmapDataJson, stage.StageIndex);
        var difficulty = "beginner"; // Can be extracted dynamically if needed

        // Call AI to generate new quiz
        var quizResult = await _aiGateway.GenerateQuizAsync(
            userId: userId,
            careerTrack: userTrack.CareerTrack.Name,
            aiStageId: stage.AiStageId,
            stageName: stage.StageName,
            difficultyLevel: difficulty,
            topics: topics
        );

        // Store as a new unsubmitted attempt
        var newAttempt = new QuizAttempt
        {
            StageProgressId = stageProgressId,
            UserId = userId,
            AttemptNumber = await _quizRepo.GetNextAttemptNumberAsync(stageProgressId),
            QuestionsDataJson = quizResult.QuestionsDataJson,
            PassingScore = quizResult.PassingScore,
            TimeLimitMinutes = quizResult.TimeLimitMinutes,
            TotalQuestions = quizResult.TotalQuestions,
            IsSubmitted = false,
            GeneratedAt = DateTime.UtcNow
        };

        await _quizRepo.CreateAttemptAsync(newAttempt);
        return _mapper.Map<QuizResponse>(newAttempt);
    }

    // =====================================================================
    // GET EXISTING QUIZ
    // =====================================================================
    public async Task<QuizResponse> GetQuizAsync(Guid quizId, string userId)
    {
        var attempt = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        if (attempt.UserId != userId)
            throw new AppException(ErrorCodes.UNAUTHORIZED, "Unauthorized access to quiz.", 403);

        return _mapper.Map<QuizResponse>(attempt);
    }

    // =====================================================================
    // GET QUESTION HINT
    // =====================================================================
    public async Task<string> GetQuestionHintAsync(Guid quizId, string questionId, int hintIndex, string userId)
    {
        var attempt = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        if (attempt.UserId != userId)
            throw new AppException(ErrorCodes.UNAUTHORIZED, "Unauthorized access to quiz.", 403);

        try
        {
            var rawQuestions = JsonSerializer.Deserialize<List<RawAIQuestion>>(attempt.QuestionsDataJson, _json);
            var q = rawQuestions?.FirstOrDefault(x => x.QuestionId == questionId);

            if (q == null || q.Hints == null)
            {
                throw new AppException(ErrorCodes.QUESTION_NOT_FOUND, "Question not found or has no hints.", 404);
            }

            var orderedHints = q.Hints.OrderBy(h => h.Level).ToList();
            if (hintIndex < 0 || hintIndex >= orderedHints.Count)
            {
                throw new AppException(ErrorCodes.QUESTION_NOT_FOUND, "No hint available for the specified index.", 404);
            }

            return orderedHints[hintIndex].Text;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse hints from quiz attempt {AttemptId}", attempt.Id);
            throw new AppException(ErrorCodes.INTERNAL_ERROR, "Failed to parse hints due to data corruption.", 500);
        }
    }

    // =====================================================================
    // SUBMIT QUIZ
    // =====================================================================
    public async Task<QuizSubmitResponse> SubmitQuizAsync(Guid quizId, SubmitQuizRequest request, string userId)
    {
        var attempt = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "No active quiz to submit.", 404);

        if (attempt.UserId != userId)
            throw new AppException(ErrorCodes.UNAUTHORIZED, "Unauthorized access to quiz.", 403);

        if (attempt.IsSubmitted)
            throw new AppException(ErrorCodes.QUIZ_ALREADY_SUBMITTED, "Quiz already submitted.", 409);

        var scoreResult = _scoring.Score(attempt.QuestionsDataJson, request.Answers, attempt.PassingScore ?? 70m);
        var isPassed = scoreResult.IsPassed;

        // Update the existing attempt with submission details
        attempt.Score = scoreResult.Score;
        attempt.IsPassed = isPassed;
        attempt.CorrectAnswers = scoreResult.CorrectAnswers;
        attempt.UserAnswersDataJson = JsonSerializer.Serialize(request.Answers);
        attempt.IsSubmitted = true;
        attempt.SubmittedAt = DateTime.UtcNow;

        await _quizRepo.UpdateAsync(attempt);

        var response = new QuizSubmitResponse
        {
            QuizId = attempt.Id,
            Score = scoreResult.Score,
            CorrectAnswers = scoreResult.CorrectAnswers,
            TotalQuestions = scoreResult.TotalQuestions,
            IsPassed = isPassed,
            SubmittedAt = attempt.SubmittedAt.Value,
            RoadmapAdapted = false
        };

        var stageProgressId = attempt.StageProgressId;
        var stage = await _stageRepo.GetByIdAsync(stageProgressId);

        if (isPassed)
        {
            if (stage != null)
            {
                await _stageRepo.CompleteStageAsync(stageProgressId);
                var nextStage = await _stageRepo.UnlockNextStageAsync(stage.RoadmapId, stage.StageIndex);
                if (nextStage != null)
                {
                    response.NextStage = new NextStageInfo
                    {
                        StageProgressId = nextStage.Id,
                        StageName = nextStage.StageName,
                        StageIndex = nextStage.StageIndex
                    };
                }
            }
        }
        else
        {
            // Quiz failed — trigger adaptation
            try
            {
                var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId);

                if (stage != null && userTrack != null)
                {
                    var (difficultyLevel, learningObjectives) = ExtractAdaptationMetadata(stage.Roadmap.RoadmapDataJson, stage.StageIndex);

                    // Build failed questions using the attempt's data
                    var failedQuestions = BuildFailedQuestions(attempt.QuestionsDataJson, attempt.UserAnswersDataJson);

                    if (failedQuestions.Count > 0)
                    {
                        var adaptResult = await _aiGateway.GetAdaptedRoadmapAsync(
                            userId: userId,
                            careerTrack: userTrack.CareerTrack.Slug,
                            aiStageId: stage.AiStageId,
                            stageName: stage.StageName,
                            difficultyLevel: difficultyLevel,
                            learningObjectives: learningObjectives,
                            failedQuestions: failedQuestions,
                            score: scoreResult.Score);

                        // Patch the stage's resources with remedial content
                        await _stageRepo.PatchResourcesAsync(stageProgressId, adaptResult.RemediationResourcesJson);

                        response.RoadmapAdapted = true;
                        _logger.LogInformation(
                            "Adaptation succeeded for stage {StageId}, user {UserId}. Topics: {Topics}",
                            stageProgressId, userId,
                            string.Join(", ", adaptResult.StrugglingTopics));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adaptation failed for stage {StageId}, user {UserId}", stageProgressId, userId);
                response.RoadmapAdapted = false;
            }
        }

        return response;
    }

    public async Task<List<QuizHistoryResponse>> GetHistoryAsync(Guid stageProgressId, string userId)
    {
        var attempts = await _quizRepo.GetAttemptsAsync(stageProgressId, userId);
        return _mapper.Map<List<QuizHistoryResponse>>(attempts);
    }

    // =====================================================================
    // ADAPTATION HELPERS
    // =====================================================================
    private static (string difficultyLevel, List<string> learningObjectives) ExtractAdaptationMetadata(string roadmapDataJson, int stageIndex)
    {
        try
        {
            using var doc = JsonDocument.Parse(roadmapDataJson);
            var root = doc.RootElement;
            string difficulty = "beginner";
            var objectives = new List<string>();

            if (root.TryGetProperty("roadmap", out var rm) &&
                rm.TryGetProperty("data", out var data))
            {
                if (data.TryGetProperty("difficulty_level", out var dl))
                    difficulty = dl.GetString() ?? "beginner";

                if (data.TryGetProperty("curriculum", out var curr) &&
                    curr.TryGetProperty("stages", out var stages))
                {
                    var stageArr = stages.EnumerateArray().ToList();
                    if (stageIndex < stageArr.Count &&
                        stageArr[stageIndex].TryGetProperty("learning_objectives", out var loEl))
                    {
                        objectives = ParseJsonStringList(loEl);
                    }
                }
            }
            return (difficulty, objectives);
        }
        catch
        {
            return ("beginner", new List<string>());
        }
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

    private static List<FailedQuestion> BuildFailedQuestions(string questionsDataJson, string userAnswersDataJson)
    {
        var result = new List<FailedQuestion>();
        try
        {
            if (string.IsNullOrWhiteSpace(questionsDataJson) || string.IsNullOrWhiteSpace(userAnswersDataJson))
                return result;

            var storedQuestions = JsonSerializer.Deserialize<List<StoredQuestionForAdaptation>>(questionsDataJson, _json) ?? new List<StoredQuestionForAdaptation>();
            var userAnswers = JsonSerializer.Deserialize<List<QuizAnswerItem>>(userAnswersDataJson, _json) ?? new List<QuizAnswerItem>();

            var answerByQuestionId = userAnswers
                .GroupBy(a => a.QuestionId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Answer, StringComparer.OrdinalIgnoreCase);

            foreach (var q in storedQuestions)
            {
                if (!answerByQuestionId.TryGetValue(q.QuestionId, out var userLabel))
                    continue;

                if (string.Equals(userLabel, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                    continue;

                var choiceMap = q.Choices
                    .GroupBy(c => c.Label, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Text, StringComparer.OrdinalIgnoreCase);

                var userAnswerText = choiceMap.GetValueOrDefault(userLabel, userLabel);
                var correctAnswerText = choiceMap.GetValueOrDefault(q.CorrectAnswer, q.CorrectAnswer);

                result.Add(new FailedQuestion
                {
                    Question = q.QuestionText,
                    UserAnswer = userAnswerText,
                    CorrectAnswer = correctAnswerText
                });
            }
        }
        catch { }
        return result;
    }

    private class StoredQuestionForAdaptation
    {
        [JsonPropertyName("question_id")] public string QuestionId { get; set; } = string.Empty;
        [JsonPropertyName("question_text")] public string QuestionText { get; set; } = string.Empty;
        [JsonPropertyName("choices")] public List<StoredChoiceForAdapt> Choices { get; set; } = new();
        [JsonPropertyName("correct_answer")] public string CorrectAnswer { get; set; } = string.Empty;
    }

    private class StoredChoiceForAdapt
    {
        [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
    }



    private static List<string> ExtractTopicsFromRoadmapJson(string roadmapDataJson, int stageIndex)
    {
        try
        {
            using var doc = JsonDocument.Parse(roadmapDataJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("roadmap", out var rm) &&
                rm.TryGetProperty("data", out var data) &&
                data.TryGetProperty("curriculum", out var curr) &&
                curr.TryGetProperty("stages", out var stages))
            {
                var stageArr = stages.EnumerateArray().ToList();
                if (stageIndex < stageArr.Count && stageArr[stageIndex].TryGetProperty("topics", out var topicsEl))
                {
                    return topicsEl.EnumerateArray()
                        .Select(t => t.GetString() ?? "")
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();
                }
            }
        }
        catch { }
        return new List<string>();
    }

    private class RawAIQuestion
    {
        [JsonPropertyName("question_id")] public string QuestionId { get; set; } = string.Empty;
        [JsonPropertyName("hints")] public List<RawAIHint> Hints { get; set; } = new();
    }

    private class RawAIHint
    {
        [JsonPropertyName("level")] public int Level { get; set; }
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
    }
}