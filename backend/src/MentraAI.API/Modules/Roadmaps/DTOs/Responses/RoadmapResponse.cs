namespace MentraAI.API.Modules.Roadmaps.DTOs.Responses;

public class RoadmapResponse
{
    public int RoadmapId { get; set; }
    public int VersionNumber { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public int TotalWeeks { get; set; }
    public List<string> SkillGaps { get; set; } = new();
    public List<StageProgressItem> Stages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class StageProgressItem
{
    public Guid StageProgressId { get; set; }  // backend GUID — used in all stage routes
    public string StageName { get; set; } = string.Empty;
    public int StageIndex { get; set; }
    public int EstimatedWeeks { get; set; }
    public string Status { get; set; } = string.Empty; // LOCKED | ACTIVE | COMPLETED
}