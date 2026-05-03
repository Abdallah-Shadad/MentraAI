namespace MentraAI.API.Modules.AIGateway.InternalModels;

public class RoadmapGenerationResult
{
    public string RoadmapDataJson { get; set; } = string.Empty; // full AI JSON stored as-is
    public string DifficultyLevel { get; set; } = string.Empty;
    public int TotalWeeks { get; set; }
    public List<string> SkillGaps { get; set; } = new();
    public List<RoadmapStage> Stages { get; set; } = new();
}

public class RoadmapStage
{
    public string AiStageId { get; set; } = string.Empty; // AI's "stage_0" — stored, never used for logic
    public string Name { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public int EstimatedWeeks { get; set; }
}