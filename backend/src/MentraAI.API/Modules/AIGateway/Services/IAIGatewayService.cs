using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Modules.AIGateway.Services;

public interface IAIGatewayService
{
    // Phase 3 — Onboarding
    Task<PredictionResult> PredictCareerAsync(
        string userId,
        UserProfile profile,
        CancellationToken ct = default);

    // Future Phase — Roadmaps (stub until then)
    Task<RoadmapGenerationResult> GenerateRoadmapAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        UserProfile profile,
        CancellationToken ct = default);

    // Future Phase — StageProgress (stub until then)
    Task<StageResourcesResult> GetStageResourcesAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string aiStageId,
        int stageIndex,
        string roadmapDataJson,
        CancellationToken ct = default);

    // Future Phase — Quizzes (stub until then)
    Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        CancellationToken ct = default);

    // Future Phase — Adaptation (stub until then)
    Task<RoadmapGenerationResult> GetAdaptedRoadmapAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        string questionsDataJson,
        string userAnswersDataJson,
        decimal score,
        CancellationToken ct = default);
}