namespace MentraAI.API.Modules.CareerTracks.DTOs.Responses;

public class MyTrackResponse
{
    public int UserTrackId { get; set; }
    public int CareerTrackId { get; set; }
    public string CareerTrackName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SelectionType { get; set; } = string.Empty;
    public DateTime SelectedAt { get; set; }
    // Frontend will use this flag to decide: show "Generate Roadmap" or "View Roadmap"
    public bool HasRoadmap { get; set; }
}