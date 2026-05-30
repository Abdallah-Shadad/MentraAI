using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
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
    // GENERATE QUIZ
    // =====================================================================
    public async Task<QuizResponse> GenerateQuizAsync(Guid stageProgressId, string userId)
    {
        // Step 1: load stage (includes Roadmap navigation property)
        var stage = await _stageRepo.GetByIdAsync(stageProgressId)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        // Step 2: verify ownership
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active career track.", 422);

        if (stage.Roadmap.UserTrackId != userTrack.Id)
            throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        // Step 3: stage must be ACTIVE
        if (stage.Status != "ACTIVE")
            throw new AppException(ErrorCodes.STAGE_NOT_ACTIVE,
                "Quiz can only be generated for an ACTIVE stage.", 422);

        // Step 4: no pending quiz for this stage
        var pending = await _quizRepo.GetPendingByStageAsync(stageProgressId);
        if (pending is not null)
            throw new AppException(ErrorCodes.QUIZ_PENDING_EXISTS,
                "A quiz is already pending for this stage. Submit it before generating a new one.", 409);

        // Step 5: extract difficulty level from stored roadmap JSON
        var difficultyLevel = ExtractDifficultyLevel(stage.Roadmap.RoadmapDataJson);

        // Step 6: extract topics for the current stage from roadmap JSON (NEW)
        var topics = ExtractStageTopics(stage.Roadmap.RoadmapDataJson, stage.StageIndex);

        // Step 7: get career track slug for AI request
        var careerTrackSlug = userTrack.CareerTrack.Slug;

        // Step 8: get next attempt number
        var attemptNumber = await _quizRepo.GetNextAttemptNumberAsync(stageProgressId);

        // Step 9: call AI with topics — AIServiceException/AIValidationException bubble to middleware
        var result = await _aiGateway.GenerateQuizAsync(
            userId: userId,
            careerTrack: careerTrackSlug,
            aiStageId: stage.AiStageId,
            stageName: stage.StageName,
            difficultyLevel: difficultyLevel,
            topics: topics);  // NEW

        // Step 10: persist — store PassingScore and TimeLimitMinutes (NEW)
        var quiz = await _quizRepo.CreateAsync(new QuizAttempt
        {
            Id = Guid.NewGuid(),
            StageProgressId = stageProgressId,
            UserId = userId,
            AttemptNumber = attemptNumber,
            IsSubmitted = false,
            QuestionsDataJson = result.QuestionsDataJson,
            TotalQuestions = result.TotalQuestions,
            PassingScore = result.PassingScore,      // NEW
            TimeLimitMinutes = result.TimeLimitMinutes,  // NEW
            GeneratedAt = DateTime.UtcNow
        });

        // Step 11: return display-only (AutoMapper strips correct_answer via resolver)
        return _mapper.Map<QuizResponse>(quiz);
    }

    // =====================================================================
    // GET QUIZ
    // =====================================================================
    public async Task<QuizResponse> GetQuizAsync(Guid quizId, string userId)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        if (quiz.UserId != userId)
            throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        return _mapper.Map<QuizResponse>(quiz);
    }

    // =====================================================================
    // SUBMIT QUIZ
    // =====================================================================
    public async Task<QuizSubmitResponse> SubmitQuizAsync(
        Guid quizId, string userId, SubmitQuizRequest request)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        if (quiz.UserId != userId)
            throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        if (quiz.IsSubmitted)
            throw new AppException(ErrorCodes.QUIZ_ALREADY_SUBMITTED,
                "This quiz has already been submitted.", 409);

        var scoreResult = _scoring.Score(quiz.QuestionsDataJson, request.Answers, quiz.PassingScore ?? 70.00m);
        var userAnswersDataJson = JsonSerializer.Serialize(request.Answers, _json);

        var submitted = await _quizRepo.SubmitAsync(
            quizId: quizId,
            userAnswersDataJson: userAnswersDataJson,
            correctAnswers: scoreResult.CorrectAnswers,
            score: scoreResult.Score,
            isPassed: scoreResult.IsPassed);

        var response = new QuizSubmitResponse
        {
            QuizId = quiz.Id,
            Score = scoreResult.Score,
            CorrectAnswers = scoreResult.CorrectAnswers,
            TotalQuestions = scoreResult.TotalQuestions,
            IsPassed = scoreResult.IsPassed,
            SubmittedAt = submitted.SubmittedAt!.Value,
            NextStage = null,
            RoadmapAdapted = false
        };

        if (scoreResult.IsPassed)
        {
            await _stageRepo.CompleteStageAsync(quiz.StageProgressId);

            var stage = await _stageRepo.GetByIdAsync(quiz.StageProgressId);
            var nextStage = await _stageRepo.UnlockNextStageAsync(
                stage!.RoadmapId, stage.StageIndex);

            if (nextStage is not null)
            {
                response.NextStage = new NextStageInfo
                {
                    StageProgressId = nextStage.Id,
                    StageName = nextStage.StageName,
                    StageIndex = nextStage.StageIndex
                };
            }
        }
        else
        {
            try
            {
                await _roadmapService.AdaptRoadmapAsync(
                    stageProgressId: quiz.StageProgressId,
                    questionsDataJson: quiz.QuestionsDataJson,
                    userAnswersDataJson: userAnswersDataJson,
                    score: scoreResult.Score,
                    userId: userId);

                response.RoadmapAdapted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Adaptation failed for stage {StageId}, user {UserId}",
                    quiz.StageProgressId, userId);
                response.RoadmapAdapted = false;
            }
        }

        return response;
    }

    // =====================================================================
    // GET HISTORY
    // =====================================================================
    public async Task<QuizHistoryResponse> GetHistoryAsync(Guid stageProgressId, string userId)
    {
        var stage = await _stageRepo.GetByIdAsync(stageProgressId)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active career track.", 422);

        if (stage.Roadmap.UserTrackId != userTrack.Id)
            throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        var attempts = await _quizRepo.GetHistoryByStageAsync(stageProgressId);

        return new QuizHistoryResponse
        {
            Attempts = _mapper.Map<List<QuizAttemptSummary>>(attempts)
        };
    }
    // =====================================================================
    // GET QUESTION HINT
    // =====================================================================
    public async Task<string> GetQuestionHintAsync(
        Guid quizId, string questionId, int hintIndex, string userId)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId);
        if (quiz == null || quiz.UserId != userId)
            throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        // Deserialize directly to a List of RawAIQuestion since the DB stores a JSON array
        var questions = JsonSerializer.Deserialize<List<RawAIQuestion>>(quiz.QuestionsDataJson, _json);
        var question = questions?.FirstOrDefault(q => q.QuestionId == questionId);

        if (question == null)
            throw new AppException(ErrorCodes.NOT_FOUND, "Question not found.", 404);

        if (question.Hints == null || !question.Hints.Any())
            throw new AppException(ErrorCodes.NOT_FOUND,
                "No hints available for this question.", 404);

        if (hintIndex < 0 || hintIndex >= question.Hints.Count)
            throw new AppException(ErrorCodes.VALIDATION_ERROR,
                "No more hints available.", 400);

        // Return the text of the hint at the requested index
        return question.Hints[hintIndex].Text;
    }

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================

    private static string ExtractDifficultyLevel(string roadmapDataJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(roadmapDataJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("roadmap", out var rm) &&
                rm.TryGetProperty("data", out var d) &&
                d.TryGetProperty("difficulty_level", out var dl))
                return dl.GetString() ?? "beginner";
        }
        catch { /* ignore */ }
        return "beginner";
    }

    /// <summary>
    /// Extracts the topics array for a specific stage from the stored roadmap JSON blob.
    /// Returns an empty list silently on any parse failure — the AI can handle empty topics gracefully.
    /// </summary>
    private static List<string> ExtractStageTopics(string roadmapDataJson, int stageIndex)
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
                if (stageIndex < stageArr.Count &&
                    stageArr[stageIndex].TryGetProperty("topics", out var topicsEl))
                {
                    return topicsEl.EnumerateArray()
                        .Select(t => t.GetString() ?? "")
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();
                }
            }
        }
        catch { /* silently return empty */ }
        return new List<string>();
    }
    // =====================================================================
    // PRIVATE CLASSES FOR HINT PARSING — matches actual AI JSON structure
    // =====================================================================
    //private class RawAIQuizData
    //{
    //    [JsonPropertyName("questions")]
    //    public List<RawAIQuestion> Questions { get; set; } = new();
    //}

    private class RawAIQuestion
    {
        [System.Text.Json.Serialization.JsonPropertyName("question_id")]
        public string QuestionId { get; set; } = string.Empty;

        // Hints are objects {level, text} — NOT plain strings
        [System.Text.Json.Serialization.JsonPropertyName("hints")]
        public List<RawAIHint> Hints { get; set; } = new();
    }

    private class RawAIHint
    {
        [System.Text.Json.Serialization.JsonPropertyName("level")]
        public int Level { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}