// Modules/Roadmaps/Services/IRoadmapService.cs
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.Roadmaps.DTOs.Responses;
using MentraAI.API.Modules.Roadmaps.Models;

namespace MentraAI.API.Modules.Roadmaps.Services;

public interface IRoadmapService
{
    Task<RoadmapResponse> GenerateRoadmapAsync(string userId);
    Task<RoadmapResponse> GetCurrentRoadmapAsync(string userId);
    Task<RoadmapHistoryResponse> GetHistoryAsync(string userId);

    // Called by QuizService when user fails a quiz — returns the new roadmap version
    Task<Roadmap> AdaptRoadmapAsync(
        Guid stageProgressId,
        List<FailedQuestion> failedQuestions,
        decimal score,
        string userId);
    Task DeactivateActiveRoadmapAsync(string userId);
}