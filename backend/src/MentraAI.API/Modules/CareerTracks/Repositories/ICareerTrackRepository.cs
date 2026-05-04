using MentraAI.API.Modules.CareerTracks.Models;

namespace MentraAI.API.Modules.CareerTracks.Repositories;
public interface ICareerTrackRepository
{
    // All active tracks ordered by Name — for list endpoint
    Task<List<CareerTrack>> GetAllActiveTracksAsync();

    // Single active track by ID — for validation in select
    Task<CareerTrack?> GetTrackByIdAsync(int careerTrackId);

    // Latest prediction for a user — for onboarding gate + selectionType logic
    Task<MLPrediction?> GetLatestPredictionByUserIdAsync(string userId);

    // Active UserTrack with CareerTrack included — used by this module AND Roadmaps module
    Task<UserTrack?> GetActiveTrackByUserIdAsync(string userId);

    // Full history of user's track selections — ordered newest first
    Task<List<UserTrack>> GetTrackHistoryByUserIdAsync(string userId);

    // Read-only join into Roadmaps table — for hasRoadmap flag in my-track response
    Task<bool> HasActiveRoadmapAsync(int userTrackId);

    // Sets all existing IsActive = true rows to false — called inside transaction
    Task DeactivateAllUserTracksAsync(string userId);

    // Inserts new UserTrack — called inside transaction, returns entity with Id set
    Task<UserTrack> InsertUserTrackAsync(UserTrack track);

    // Convenience method to do both of the above in one call s
    Task<UserTrack> ReplaceActiveTrackAsync(string userId, UserTrack newTrack);
}