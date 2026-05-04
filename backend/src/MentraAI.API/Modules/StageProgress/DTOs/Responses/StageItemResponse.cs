namespace MentraAI.API.Modules.StageProgress.DTOs.Responses;

public class StageItemResponse
{
    public Guid StageProgressId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StageIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasResources { get; set; }
}