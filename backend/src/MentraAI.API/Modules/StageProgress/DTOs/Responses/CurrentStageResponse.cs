namespace MentraAI.API.Modules.StageProgress.DTOs.Responses;

public class CurrentStageResponse
{
    public Guid StageProgressId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StageIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasResources { get; set; }  // true if ResourcesDataJson != null (stage entered)
    public bool HasPendingQuiz { get; set; }  // true if a quiz exists with IsSubmitted = false
}