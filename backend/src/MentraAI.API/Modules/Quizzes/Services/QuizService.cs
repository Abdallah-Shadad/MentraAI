using System.Text.Json;
using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;
using MentraAI.API.Modules.Quizzes.Models;
using MentraAI.API.Modules.Quizzes.Repositories;
using MentraAI.API.Modules.Roadmaps.Services;
using MentraAI.API.Modules.StageProgress.Repositories;

namespace MentraAI.API.Modules.Quizzes.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository          _quizRepo;
    private readonly IStageProgressRepository _stageRepo;
    private readonly ICareerTrackRepository   _trackRepo;
    private readonly IAIGatewayService        _aiGateway;
    private readonly IQuizScoringService      _scoring;
    private readonly IRoadmapService          _roadmapService;
    private readonly IMapper                  _mapper;
    private readonly ILogger<QuizService>     _logger;

    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public QuizService(
        IQuizRepository          quizRepo,
        IStageProgressRepository stageRepo,
        ICareerTrackRepository   trackRepo,
        IAIGatewayService        aiGateway,
        IQuizScoringService      scoring,
        IRoadmapService          roadmapService,
        IMapper                  mapper,
        ILogger<QuizService>     logger)
    {
        _quizRepo       = quizRepo;
        _stageRepo      = stageRepo;
        _trackRepo      = trackRepo;
        _aiGateway      = aiGateway;
        _scoring        = scoring;
        _roadmapService = roadmapService;
        _mapper         = mapper;
        _logger         = logger;
    }

    // =====================================================================
    // GENERATE QUIZ
    // =====================================================================
    public async Task<QuizResponse> GenerateQuizAsync(Guid stageProgressId, string userId)
    {
        // Step 1: load stage
        var stage = await _stageRepo.GetByIdAsync(stageProgressId)
            ?? throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        // Step 2: verify ownership — stage must belong to user's active roadmap
        var userTrack = await _trackRepo.GetActiveTrackByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NO_ACTIVE_TRACK, "No active career track.", 422);

        if (stage.Roadmap.UserTrackId != userTrack.Id)
            throw new AppException(ErrorCodes.STAGE_NOT_FOUND, "Stage not found.", 404);

        // Step 3: stage must be ACTIVE
        if (stage.Status != "ACTIVE")
            throw new AppException(ErrorCodes.STAGE_NOT_ACTIVE,
                "Quiz can only be generated for an ACTIVE stage.", 422);

        // Step 4: no pending (unsubmitted) quiz for this stage
        var pending = await _quizRepo.GetPendingByStageAsync(stageProgressId);
        if (pending is not null)
            throw new AppException(ErrorCodes.QUIZ_PENDING_EXISTS,
                "A quiz is already pending for this stage. Submit it before generating a new one.", 409);

        // Step 5: extract difficulty level from stored roadmap JSON
        var difficultyLevel = ExtractDifficultyLevel(stage.Roadmap.RoadmapDataJson);

        // Step 6: get career track slug for AI request
        var careerTrackSlug = userTrack.CareerTrack.Slug;

        // Step 7: get next attempt number
        var attemptNumber = await _quizRepo.GetNextAttemptNumberAsync(stageProgressId);

        // Step 8: call AI — AIServiceException / AIValidationException bubble to middleware
        var result = await _aiGateway.GenerateQuizAsync(
            userId:          userId,
            careerTrack:     careerTrackSlug,
            aiStageId:       stage.AiStageId,
            stageName:       stage.StageName,
            difficultyLevel: difficultyLevel);

        // Step 9: persist — QuestionsDataJson stores full questions WITH correct_answer
        var quiz = await _quizRepo.CreateAsync(new QuizAttempt
        {
            Id                = Guid.NewGuid(),
            StageProgressId   = stageProgressId,
            UserId            = userId,
            AttemptNumber     = attemptNumber,
            IsSubmitted       = false,
            QuestionsDataJson = result.QuestionsDataJson,
            TotalQuestions    = result.TotalQuestions,
            GeneratedAt       = DateTime.UtcNow
        });

        // Step 10: return display-only — strip correct_answer
        return _mapper.Map<QuizResponse>(quiz);
    }

    // =====================================================================
    // GET QUIZ
    // =====================================================================
    public async Task<QuizResponse> GetQuizAsync(Guid quizId, string userId)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        // Never reveal someone else's quiz exists
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
        // Step 1: load and verify ownership
        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        if (quiz.UserId != userId)
            throw new AppException(ErrorCodes.QUIZ_NOT_FOUND, "Quiz not found.", 404);

        // Step 2: prevent re-submission
        if (quiz.IsSubmitted)
            throw new AppException(ErrorCodes.QUIZ_ALREADY_SUBMITTED,
                "This quiz has already been submitted.", 409);

        // Step 3: score
        var scoreResult = _scoring.Score(quiz.QuestionsDataJson, request.Answers);

        // Step 4: serialize user answers for storage
        var userAnswersDataJson = JsonSerializer.Serialize(request.Answers, _json);

        // Step 5: persist submission
        var submitted = await _quizRepo.SubmitAsync(
            quizId:              quizId,
            userAnswersDataJson: userAnswersDataJson,
            correctAnswers:      scoreResult.CorrectAnswers,
            score:               scoreResult.Score,
            isPassed:            scoreResult.IsPassed);

        // Step 6: build response base
        var response = new QuizSubmitResponse
        {
            QuizId         = quiz.Id,
            Score          = scoreResult.Score,
            CorrectAnswers = scoreResult.CorrectAnswers,
            TotalQuestions = scoreResult.TotalQuestions,
            IsPassed       = scoreResult.IsPassed,
            SubmittedAt    = submitted.SubmittedAt!.Value,
            NextStage      = null,
            RoadmapAdapted = false
        };

        if (scoreResult.IsPassed)
        {
            // Complete the current stage
            await _stageRepo.CompleteStageAsync(quiz.StageProgressId);

            // Unlock the next stage (returns null if this was the last stage)
            var stage    = await _stageRepo.GetByIdAsync(quiz.StageProgressId);
            var nextStage = await _stageRepo.UnlockNextStageAsync(
                stage!.RoadmapId, stage.StageIndex);

            if (nextStage is not null)
            {
                response.NextStage = new NextStageInfo
                {
                    StageProgressId = nextStage.Id,
                    StageName       = nextStage.StageName,
                    StageIndex      = nextStage.StageIndex
                };
            }
        }
        else
        {
            // Adaptation failure must NEVER fail the submission
            // Score is already saved — user has their result regardless
            try
            {
                await _roadmapService.AdaptRoadmapAsync(
                    stageProgressId:   quiz.StageProgressId,
                    questionsDataJson: quiz.QuestionsDataJson,
                    userAnswersDataJson: userAnswersDataJson,
                    score:             scoreResult.Score,
                    userId:            userId);

                response.RoadmapAdapted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Adaptation failed for stage {StageId}, user {UserId}",
                    quiz.StageProgressId, userId);

                response.RoadmapAdapted = false;
                // Do NOT rethrow — submission already persisted
            }
        }

        return response;
    }

    // =====================================================================
    // GET HISTORY
    // =====================================================================
    public async Task<QuizHistoryResponse> GetHistoryAsync(Guid stageProgressId, string userId)
    {
        // Verify stage belongs to user
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

}
