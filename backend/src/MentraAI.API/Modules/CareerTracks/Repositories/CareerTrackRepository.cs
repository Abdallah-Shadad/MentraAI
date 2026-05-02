using Microsoft.EntityFrameworkCore;
using MentraAI.API.Data;
using MentraAI.API.Modules.CareerTracks.Models;

namespace MentraAI.API.Modules.CareerTracks.Repositories;

public class CareerTrackRepository : ICareerTrackRepository
{
    private readonly AppDbContext _db;

    public CareerTrackRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CareerTrack>> GetAllActiveTracksAsync() =>
        await _db.CareerTracks
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public async Task<CareerTrack?> GetTrackByIdAsync(int careerTrackId) =>
        await _db.CareerTracks
        .FirstOrDefaultAsync(t => t.Id == careerTrackId && t.IsActive);

    public async Task<MLPrediction?> GetLatestPredictionByUserIdAsync(string userId) =>
    await _db.MLPredictions
        .Where(p => p.UserId == userId)
        .OrderByDescending(p => p.CreatedAt)
        .FirstOrDefaultAsync();

    public async Task<UserTrack?> GetActiveTrackByUserIdAsync(string userId) =>
        await _db.UserTracks
            .Include(t => t.CareerTrack)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.IsActive);

    // Full history of user's track selections — ordered newest first
    public async Task<List<UserTrack>> GetTrackHistoryByUserIdAsync(string userId) =>
        await _db.UserTracks
            .Include(t => t.CareerTrack)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.SelectedAt)
            .ToListAsync();

    // Read-only join into Roadmaps — this module never writes Roadmaps
    public async Task<bool> HasActiveRoadmapAsync(int userTrackId) =>
        await _db.Roadmaps
            .AnyAsync(r => r.UserTrackId == userTrackId && r.IsActive);

    public async Task DeactivateAllUserTracksAsync(string userId)
    {
        // ExecuteUpdateAsync skips change tracking — fast bulk update
        await _db.UserTracks
            .Where(t => t.UserId == userId && t.IsActive)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));
    }

    public async Task<UserTrack> InsertUserTrackAsync(UserTrack track)
    {
        _db.UserTracks.Add(track);
        await _db.SaveChangesAsync();
        return track;
    }
}