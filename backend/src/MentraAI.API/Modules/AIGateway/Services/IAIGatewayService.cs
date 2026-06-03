using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.Chat.DTOs.Requests;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Modules.AIGateway.Services;

public interface IAIGatewayService
{
    // Onboarding — career prediction
    Task<PredictionResult> PredictCareerAsync(
        string userId,
        UserProfile profile,
        CancellationToken ct = default);

    // Track Recommender (Phase 6)
    Task<TrackRecommendationResult> GetTrackRecommendationsAsync(
        string userId,
        TrackRecommendProfile profile,
        CancellationToken ct = default);

    // Roadmaps — Mode 1
    Task<RoadmapGenerationResult> GenerateRoadmapAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string userBackground,
        List<string> currentSkills,
        CancellationToken ct = default);

    // Stage Resources — Mode 2 (updated signature)
    Task<StageResourcesResult> GetStageResourcesAsync(
        string userId,
        string careerTrack,
        int weeklyHours,
        string aiStageId,
        string stageName,
        List<string> topics,
        List<string> learningObjectives,
        int estimatedWeeks,
        CancellationToken ct = default);

    // Quizzes
    Task<QuizGenerationResult> GenerateQuizAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        List<string> topics,
        CancellationToken ct = default);

    // Adaptation — Mode 3 (updated signature — replaces old GetAdaptedRoadmapAsync)
    Task<AdaptationResult> GetAdaptedRoadmapAsync(
        string userId,
        string careerTrack,
        string aiStageId,
        string stageName,
        string difficultyLevel,
        List<string> learningObjectives,
        List<FailedQuestion> failedQuestions,
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
}