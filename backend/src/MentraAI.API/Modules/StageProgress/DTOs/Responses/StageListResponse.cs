namespace MentraAI.API.Modules.StageProgress.DTOs.Responses;

public class StageListResponse
{
    public List<StageItemResponse> Stages { get; set; } = new();
}