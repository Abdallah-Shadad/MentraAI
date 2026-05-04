// Modules/Roadmaps/DTOs/Responses/RoadmapSummaryResponse.cs
namespace MentraAI.API.Modules.Roadmaps.DTOs.Responses;

public class RoadmapSummaryResponse
{
    public int RoadmapId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}