namespace MentraAI.API.Modules.CareerTracks.DTOs.Responses;

public class SelectTrackResponse
{
    public int UserTrackId { get; set; }
    public int CareerTrackId { get; set; }
    public string CareerTrackName { get; set; } = string.Empty;
    public string SelectionType { get; set; } = string.Empty;
    public DateTime SelectedAt { get; set; }
}