// Modules/AIGateway/InternalModels/StageResourcesResult.cs
namespace MentraAI.API.Modules.AIGateway.InternalModels;

public class StageResourcesResult
{
    // Full resources JSON stored as-is in UserStageProgress.ResourcesDataJson
    public string ResourcesDataJson { get; set; } = string.Empty;
}