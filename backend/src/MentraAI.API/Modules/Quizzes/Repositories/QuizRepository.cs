using Microsoft.EntityFrameworkCore;
using MentraAI.API.Data;
using MentraAI.API.Modules.Quizzes.Models;

namespace MentraAI.API.Modules.Quizzes.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _db;

    public QuizRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<QuizAttempt?> GetByIdAsync(Guid quizId) =>
        await _db.QuizAttempts.FirstOrDefaultAsync(q => q.Id == quizId);

    public async Task<QuizAttempt?> GetPendingByStageAsync(Guid stageProgressId) =>
        await _db.QuizAttempts
            .FirstOrDefaultAsync(q => q.StageProgressId == stageProgressId && !q.IsSubmitted);

    public async Task<int> GetNextAttemptNumberAsync(Guid stageProgressId)
    {
        var max = await _db.QuizAttempts
            .Where(q => q.StageProgressId == stageProgressId)
            .MaxAsync(q => (int?)q.AttemptNumber);
        return (max ?? 0) + 1;
    }

    public async Task<QuizAttempt> CreateAsync(QuizAttempt quiz)
    {
        _db.QuizAttempts.Add(quiz);
        await _db.SaveChangesAsync();
        return quiz;
    }

    public async Task<QuizAttempt> SubmitAsync(
        Guid quizId,
        string userAnswersDataJson,
        int correctAnswers,
        decimal score,
        bool isPassed)
    {
        var quiz = await _db.QuizAttempts.FirstOrDefaultAsync(q => q.Id == quizId)
            ?? throw new InvalidOperationException($"QuizAttempt {quizId} not found during submit.");

        quiz.UserAnswersDataJson = userAnswersDataJson;
        quiz.CorrectAnswers      = correctAnswers;
        quiz.Score               = score;
        quiz.IsPassed            = isPassed;
        quiz.IsSubmitted         = true;
        quiz.SubmittedAt         = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return quiz;
    }

    public async Task<List<QuizAttempt>> GetHistoryByStageAsync(Guid stageProgressId) =>
        await _db.QuizAttempts
            .Where(q => q.StageProgressId == stageProgressId)
            .OrderBy(q => q.AttemptNumber)
            .ToListAsync();
}
