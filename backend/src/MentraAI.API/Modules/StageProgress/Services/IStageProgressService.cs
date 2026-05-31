using MentraAI.API.Modules.StageProgress.DTOs.Responses;

namespace MentraAI.API.Modules.StageProgress.Services;

public interface IStageProgressService
{
    Task<StageListResponse> GetAllStagesAsync(string userId);
    Task<CurrentStageResponse> GetCurrentStageAsync(string userId);
    Task<StageResourcesResponse> EnterStageAsync(Guid stageProgressId, string userId);
    Task<StageResourcesResponse> GetResourcesAsync(Guid stageProgressId, string userId);
}