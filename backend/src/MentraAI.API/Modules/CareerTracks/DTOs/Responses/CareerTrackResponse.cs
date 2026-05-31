namespace MentraAI.API.Modules.CareerTracks.DTOs.Responses;

public class CareerTrackResponse
{
    public int CareerTrackId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}