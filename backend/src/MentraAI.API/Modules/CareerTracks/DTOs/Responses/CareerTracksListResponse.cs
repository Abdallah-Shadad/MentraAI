namespace MentraAI.API.Modules.CareerTracks.DTOs.Responses;

public class CareerTracksListResponse
{
    public List<CareerTrackResponse> Tracks { get; set; } = new();
}