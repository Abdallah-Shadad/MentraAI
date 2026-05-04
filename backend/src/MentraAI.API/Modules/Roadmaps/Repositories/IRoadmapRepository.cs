using MentraAI.API.Modules.Roadmaps.Models;
using MentraAI.API.Modules.StageProgress.Models;

namespace MentraAI.API.Modules.Roadmaps.Repositories;

public interface IRoadmapRepository
{
    // Check if user's active track already has an active roadmap (for 409 guard)
    Task<bool> HasActiveRoadmapAsync(int userTrackId);

    // Get the current active roadmap with its stage progress rows
    Task<Roadmap?> GetActiveRoadmapAsync(int userTrackId);

    // Get all roadmap versions for a user track — ordered oldest first
    Task<List<Roadmap>> GetAllVersionsAsync(int userTrackId);

    // Get max version number — used when creating adaptation version
    Task<int> GetMaxVersionAsync(int userTrackId);

    // Insert new roadmap row — returns entity with Id set
    Task<Roadmap> CreateAsync(Roadmap roadmap);

    // Update a roadmap row (used to flip IsActive = false on adaptation)
    Task UpdateAsync(Roadmap roadmap);
    // Temporary stub in RoadmapRepository until Quizzes module is built
    public Task<bool> HasPendingQuizAsync(Guid stageProgressId)
        => Task.FromResult(false);

    // new method to create a new roadmap version based on an old one, with new stage progress rows
    Task<Roadmap> CreateNewVersionAsync(
        Roadmap oldRoadmap,
        Roadmap newRoadmap,
        List<UserStageProgress> newStages);
}