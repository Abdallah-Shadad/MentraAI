// Modules/StageProgress/Repositories/StageProgressRepository.cs
using Microsoft.EntityFrameworkCore;
using MentraAI.API.Data;
using MentraAI.API.Modules.StageProgress.Models;

namespace MentraAI.API.Modules.StageProgress.Repositories;

public class StageProgressRepository : IStageProgressRepository
{
    private readonly AppDbContext _db;

    public StageProgressRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(UserStageProgress stage)
    {
        _db.UserStageProgress.Add(stage);
        await _db.SaveChangesAsync();
    }

    public async Task<List<UserStageProgress>> GetByRoadmapIdAsync(int roadmapId) =>
        await _db.UserStageProgress
            .Where(s => s.RoadmapId == roadmapId)
            .OrderBy(s => s.StageIndex)
            .ToListAsync();

    public async Task<UserStageProgress?> GetByIdAsync(Guid stageProgressId) =>
        await _db.UserStageProgress
            .Include(s => s.Roadmap)
            .FirstOrDefaultAsync(s => s.Id == stageProgressId);

    public async Task<UserStageProgress?> GetActiveStageAsync(int roadmapId) =>
        await _db.UserStageProgress
            .FirstOrDefaultAsync(s => s.RoadmapId == roadmapId && s.Status == "ACTIVE");

    public async Task UpdateAsync(UserStageProgress stage)
    {
        _db.UserStageProgress.Update(stage);
        await _db.SaveChangesAsync();
    }

    public async Task CompleteStageAsync(Guid stageProgressId)
    {
        var stage = await _db.UserStageProgress
            .FirstOrDefaultAsync(s => s.Id == stageProgressId);

        if (stage is null) return;

        stage.Status = "COMPLETED";
        stage.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<UserStageProgress?> UnlockNextStageAsync(int roadmapId, int currentStageIndex)
    {
        var next = await _db.UserStageProgress
            .FirstOrDefaultAsync(s =>
                s.RoadmapId == roadmapId &&
                s.StageIndex == currentStageIndex + 1);

        if (next is null) return null;

        next.Status = "ACTIVE";
        next.UnlockedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return next;
    }
}