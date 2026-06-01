namespace MentraAI.API.Modules.Roadmaps.DTOs.Requests;

public class RoadmapGenerateRequest
{
    public string CareerTrackSlug { get; set; } = string.Empty;
    public int WeeklyHours { get; set; }
}