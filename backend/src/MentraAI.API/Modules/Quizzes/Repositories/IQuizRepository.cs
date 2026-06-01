using MentraAI.API.Modules.Quizzes.Models;

namespace MentraAI.API.Modules.Quizzes.Repositories;

public interface IQuizRepository
{
    Task<QuizAttempt?> GetByIdAsync(Guid attemptId);

    // Gets the active (unsubmitted) quiz attempt for a stage
    Task<QuizAttempt?> GetActiveQuizByStageIdAsync(Guid stageProgressId);

    Task<int> GetNextAttemptNumberAsync(Guid stageProgressId);

    Task<QuizAttempt> CreateAttemptAsync(QuizAttempt attempt);

    Task<QuizAttempt> UpdateAsync(QuizAttempt attempt);

    Task<List<QuizAttempt>> GetAttemptsAsync(Guid stageProgressId, string userId);
}