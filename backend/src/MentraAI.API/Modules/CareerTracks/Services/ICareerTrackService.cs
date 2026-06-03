using MentraAI.API.Modules.CareerTracks.DTOs.Responses;
using MentraAI.API.Modules.CareerTracks.DTOs.Requests;

namespace MentraAI.API.Modules.CareerTracks.Services;

public interface ICareerTrackService
{
    Task<CareerTracksListResponse> GetAllTracksAsync();
    Task<PredictionResponse> GetPredictionAsync(string userId);
    Task<SelectTrackResponse> SelectTrackAsync(string userId, SelectTrackRequest request, CancellationToken ct = default);
    Task<MyTrackResponse> GetMyTrackAsync(string userId);

    // Phase 6 — Track Recommender: profile is fetched internally from UserProfiles.
    Task<TrackRecommendationResponse> GetRecommendationsAsync(string userId, CancellationToken ct = default);
}