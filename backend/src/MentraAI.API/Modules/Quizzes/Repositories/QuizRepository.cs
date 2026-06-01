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

    public async Task<QuizAttempt?> GetByIdAsync(Guid attemptId)
    {
        return await _db.QuizAttempts.FirstOrDefaultAsync(q => q.Id == attemptId);
    }

    public async Task<QuizAttempt?> GetActiveQuizByStageIdAsync(Guid stageProgressId)
    {
        return await _db.QuizAttempts
            .FirstOrDefaultAsync(q => q.StageProgressId == stageProgressId && !q.IsSubmitted);
    }

    public async Task<int> GetNextAttemptNumberAsync(Guid stageProgressId)
    {
        var max = await _db.QuizAttempts
            .Where(q => q.StageProgressId == stageProgressId)
            .MaxAsync(q => (int?)q.AttemptNumber);

        return (max ?? 0) + 1;
    }

    public async Task<QuizAttempt> CreateAttemptAsync(QuizAttempt attempt)
    {
        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync();
        return attempt;
    }

    public async Task<QuizAttempt> UpdateAsync(QuizAttempt attempt)
    {
        _db.QuizAttempts.Update(attempt);
        await _db.SaveChangesAsync();
        return attempt;
    }

    public async Task<List<QuizAttempt>> GetAttemptsAsync(Guid stageProgressId, string userId)
    {
        return await _db.QuizAttempts
            .Where(q => q.StageProgressId == stageProgressId && q.UserId == userId)
            .OrderBy(q => q.AttemptNumber)
            .ToListAsync();
    }
}