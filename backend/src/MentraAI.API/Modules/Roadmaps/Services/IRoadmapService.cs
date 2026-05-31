// Modules/Roadmaps/Services/IRoadmapService.cs
using MentraAI.API.Modules.Roadmaps.DTOs.Responses;
using MentraAI.API.Modules.Roadmaps.Models;

namespace MentraAI.API.Modules.Roadmaps.Services;

public interface IRoadmapService
{
    Task<RoadmapResponse> GenerateRoadmapAsync(string userId);
    Task<RoadmapResponse> GetCurrentRoadmapAsync(string userId);
    Task<RoadmapHistoryResponse> GetHistoryAsync(string userId);
}