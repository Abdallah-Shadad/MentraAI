// Modules/StageProgress/Repositories/IStageProgressRepository.cs
using MentraAI.API.Modules.StageProgress.Models;

namespace MentraAI.API.Modules.StageProgress.Repositories;

public interface IStageProgressRepository
{
    // ── Called by Roadmaps module (on roadmap generate + adaptation) ──────
    Task CreateAsync(UserStageProgress stage);
    Task<List<UserStageProgress>> GetByRoadmapIdAsync(int roadmapId);

    // ── Used inside this module ───────────────────────────────────────────

    // Find a stage by its backend GUID — used in enter + resources endpoints
    Task<UserStageProgress?> GetByIdAsync(Guid stageProgressId);

    // Get the single ACTIVE stage for a roadmap
    Task<UserStageProgress?> GetActiveStageAsync(int roadmapId);

    // Update a stage row — used to cache ResourcesDataJson after AI call
    Task UpdateAsync(UserStageProgress stage);

    // ── Called by Quizzes module ──────────────────────────────────────────

    // Complete a stage (Status = COMPLETED, CompletedAt = now) — called on quiz pass
    Task CompleteStageAsync(Guid stageProgressId);

    // Unlock the next stage by StageIndex + 1 — called on quiz pass
    // Returns the unlocked stage (or null if it was the last stage)
    Task<UserStageProgress?> UnlockNextStageAsync(int roadmapId, int currentStageIndex);

    // Temporary stub until Quizzes module is built — always returns false for now
    Task<bool> HasPendingQuizAsync(Guid stageProgressId);
}