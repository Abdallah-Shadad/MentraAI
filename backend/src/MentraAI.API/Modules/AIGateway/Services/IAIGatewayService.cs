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

    // Quizzes — updated signature with topics
    Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        List<string> topics,
        CancellationToken ct = default);

    // Adaptation
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

    // Chat — NEW
    Task StreamChatAsync(
        ChatAIRequest request,
        HttpResponse httpResponse,
        CancellationToken ct = default);

    // Chat memory delete — NEW
    Task DeleteChatMemoryAsync(
        string userId,
        string conversationId,
        CancellationToken ct = default);
}