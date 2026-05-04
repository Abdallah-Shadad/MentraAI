namespace MentraAI.API.Modules.Roadmaps.DTOs.Responses;

public class RoadmapHistoryResponse
{
    public List<RoadmapSummaryResponse> Versions { get; set; } = new();
}