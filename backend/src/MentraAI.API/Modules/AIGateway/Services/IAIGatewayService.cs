using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.Chat.DTOs.Requests;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Modules.AIGateway.Services;

public interface IAIGatewayService
{
    // Phase 3 — Onboarding
    Task<PredictionResult> PredictCareerAsync(
        string userId,
        UserProfile profile,
        CancellationToken ct = default);

    // Roadmaps
    Task<RoadmapGenerationResult> GenerateRoadmapAsync(
        string userId,
        string careerTrackSlug,
        int weeklyHours,
        string userBackground,
        List<string> currentSkills,
        CancellationToken ct = default);

    // StageProgress
    Task<StageResourcesResult> GetStageResourcesAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string aiStageId,
        int stageIndex,
        string roadmapDataJson,
        CancellationToken ct = default);

    // Quizzes — includes topics parameter added in Phase 5
    Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        List<string> topics,
        CancellationToken ct = default);

    // Adaptation — Phase 6 stub
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

    // Chat — SSE stream proxy
    Task StreamChatAsync(
        ChatAIRequest request,
        HttpResponse httpResponse,
        CancellationToken ct = default);

    // Chat — Redis memory delete
    Task DeleteChatMemoryAsync(
        string userId,
        string conversationId,
        CancellationToken ct = default);

    // Chat health check
    Task<bool> CheckChatHealthAsync();

    // Phase 6 — Track Recommender (NEW)
    Task<TrackRecommendationResult> GetTrackRecommendationsAsync(
        string userId,
        TrackRecommendProfile profile,
        CancellationToken ct = default);
}