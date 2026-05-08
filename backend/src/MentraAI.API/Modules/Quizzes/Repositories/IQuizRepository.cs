using MentraAI.API.Modules.Quizzes.Models;

namespace MentraAI.API.Modules.Quizzes.Repositories;

public interface IQuizRepository
{
    // Simple PK lookup — no includes needed
    Task<QuizAttempt?> GetByIdAsync(Guid quizId);

    // Returns unsubmitted quiz for this stage — used for idempotency check
    Task<QuizAttempt?> GetPendingByStageAsync(Guid stageProgressId);

    // MAX(AttemptNumber) + 1, returns 1 if no previous attempts
    Task<int> GetNextAttemptNumberAsync(Guid stageProgressId);

    // Insert + SaveChanges, returns entity with Id set
    Task<QuizAttempt> CreateAsync(QuizAttempt quiz);

    // Updates 6 fields on existing row, SaveChanges, returns updated entity
    Task<QuizAttempt> SubmitAsync(
        Guid quizId,
        string userAnswersDataJson,
        int correctAnswers,
        decimal score,
        bool isPassed);

    // All attempts for a stage, ordered oldest first
    Task<List<QuizAttempt>> GetHistoryByStageAsync(Guid stageProgressId);
}
