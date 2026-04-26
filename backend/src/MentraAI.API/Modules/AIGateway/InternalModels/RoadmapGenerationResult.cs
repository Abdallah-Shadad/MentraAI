namespace MentraAI.API.Modules.AIGateway.InternalModels;

// Placeholder — full implementation in Phase 3 (Roadmaps module)
public class RoadmapGenerationResult
{
    public string DifficultyLevel { get; set; } = string.Empty;
    public int TotalWeeks { get; set; }
    public string RoadmapDataJson { get; set; } = string.Empty;
    public List<StageInfo> Stages { get; set; } = new();
}

public class StageInfo
{
    // AI's own stage id string — stored in UserStageProgress.AiStageId.
    // Only used when sending requests back to AI. Never exposed to frontend.
    public string AiStageId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int EstimatedWeeks { get; set; }
}