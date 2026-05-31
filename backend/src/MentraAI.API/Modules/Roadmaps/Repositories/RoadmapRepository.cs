// Modules/Roadmaps/Repositories/RoadmapRepository.cs
using MentraAI.API.Data;
using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.StageProgress.Models;
using Microsoft.EntityFrameworkCore;

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

    // Method to create a new roadmap version based on an old one, with new stage progress rows
    public async Task<Roadmap> CreateNewVersionAsync(
    Roadmap oldRoadmap,
    Roadmap newRoadmap,
    List<UserStageProgress> newStages)
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Deactivate current roadmap
                oldRoadmap.IsActive = false;
                _db.Roadmaps.Update(oldRoadmap);

                // Insert new roadmap version
                _db.Roadmaps.Add(newRoadmap);
                await _db.SaveChangesAsync(); // To populate newRoadmap.Id

                // Insert new stages tied to the new roadmap
                foreach (var stage in newStages)
                {
                    stage.RoadmapId = newRoadmap.Id;
                    _db.UserStageProgress.Add(stage);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return newRoadmap;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }
    public async Task<Roadmap> CreateWithStagesAsync(Roadmap roadmap, List<UserStageProgress> stages)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Add roadmap first to get the generated Id for the stages
                _db.Roadmaps.Add(roadmap);
                await _db.SaveChangesAsync();

                // Link stages to the new roadmap
                foreach (var stage in stages)
                {
                    stage.RoadmapId = roadmap.Id;
                }
                _db.UserStageProgress.AddRange(stages); // AddRange that accepts a list of stages

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return roadmap;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }
}