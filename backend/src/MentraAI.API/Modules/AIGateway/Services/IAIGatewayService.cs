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

    // Future Phase — Roadmaps
    Task<RoadmapGenerationResult> GenerateRoadmapAsync(
        string userId,
        string careerTrackSlug,
        int weeklyHours,
        string userBackground,
        List<string> currentSkills,
        CancellationToken ct = default); // Added CancellationToken

    // Future Phase — StageProgress
    Task<StageResourcesResult> GetStageResourcesAsync(
            string userId,
            string careerTrack,
            int weeklyHours,
            string aiStageId,
            int stageIndex,
            string roadmapDataJson,
            CancellationToken ct = default);

    // Future Phase — Quizzes
    Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        CancellationToken ct = default); // Added CancellationToken[cite: 2]

    // Future Phase — Adaptation
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