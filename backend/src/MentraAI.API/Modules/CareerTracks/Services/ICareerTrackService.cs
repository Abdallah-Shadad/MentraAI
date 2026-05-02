using MentraAI.API.Modules.CareerTracks.DTOs.Requests;
using MentraAI.API.Modules.CareerTracks.DTOs.Responses;

namespace MentraAI.API.Modules.CareerTracks.Services;

public interface ICareerTrackService
{
    Task<CareerTracksListResponse> GetAllTracksAsync();
    Task<PredictionResponse> GetPredictionAsync(string userId);
    Task<SelectTrackResponse> SelectTrackAsync(string userId, SelectTrackRequest request);
    Task<MyTrackResponse> GetMyTrackAsync(string userId);
}