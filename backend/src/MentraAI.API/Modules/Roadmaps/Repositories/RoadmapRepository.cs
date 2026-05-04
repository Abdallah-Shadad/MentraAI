// Modules/Roadmaps/Repositories/RoadmapRepository.cs
using Microsoft.EntityFrameworkCore;
using MentraAI.API.Data;
using MentraAI.API.Modules.Roadmaps.Models;

namespace MentraAI.API.Modules.Roadmaps.Repositories;

public class RoadmapRepository : IRoadmapRepository
{
    private readonly AppDbContext _db;

    public RoadmapRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasActiveRoadmapAsync(int userTrackId) =>
        await _db.Roadmaps
            .AnyAsync(r => r.UserTrackId == userTrackId && r.IsActive);

    public async Task<Roadmap?> GetActiveRoadmapAsync(int userTrackId) =>
        await _db.Roadmaps
            .Include(r => r.UserTrack)
            .FirstOrDefaultAsync(r => r.UserTrackId == userTrackId && r.IsActive);

    public async Task<List<Roadmap>> GetAllVersionsAsync(int userTrackId) =>
        await _db.Roadmaps
            .Where(r => r.UserTrackId == userTrackId)
            .OrderBy(r => r.VersionNumber)
            .ToListAsync();

    public async Task<int> GetMaxVersionAsync(int userTrackId) =>
        await _db.Roadmaps
            .Where(r => r.UserTrackId == userTrackId)
            .MaxAsync(r => (int?)r.VersionNumber) ?? 0;

    public async Task<Roadmap> CreateAsync(Roadmap roadmap)
    {
        _db.Roadmaps.Add(roadmap);
        await _db.SaveChangesAsync();
        return roadmap;
    }

    public async Task UpdateAsync(Roadmap roadmap)
    {
        _db.Roadmaps.Update(roadmap);
        await _db.SaveChangesAsync();
    }
}