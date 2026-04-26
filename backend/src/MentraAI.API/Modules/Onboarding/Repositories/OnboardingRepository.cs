using Microsoft.EntityFrameworkCore;
using MentraAI.API.Data;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.Onboarding.Models;

namespace MentraAI.API.Modules.Onboarding.Repositories;

public class OnboardingRepository : IOnboardingRepository
{
    private readonly AppDbContext _db;

    public OnboardingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<OnboardingQuestion>> GetAllActiveQuestionsAsync() =>
        await _db.OnboardingQuestions
            .Where(q => q.IsActive)
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync();

    public async Task<List<OnboardingQuestion>> GetQuestionsByIdsAsync(List<int> ids) =>
        await _db.OnboardingQuestions
            .Where(q => ids.Contains(q.Id) && q.IsActive)
            .ToListAsync();

    public async Task<List<OnboardingAnswer>> GetAnswersByUserIdAsync(string userId) =>
        await _db.OnboardingAnswers
            .Include(a => a.Question)
            .Where(a => a.UserId == userId)
            .ToListAsync();

    public async Task<int> GetAnswerCountByUserIdAsync(string userId) =>
    await _db.OnboardingAnswers
        .Where(a => a.UserId == userId && a.Question.IsActive)
        .CountAsync();

    public async Task UpsertAnswersAsync(string userId, List<OnboardingAnswer> answers)
    {
        var existing = await _db.OnboardingAnswers
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var existingMap = existing.ToDictionary(a => a.QuestionId);

        foreach (var incoming in answers)
        {
            if (existingMap.TryGetValue(incoming.QuestionId, out var row))
            {
                row.AnswerText = incoming.AnswerText;
                row.AnsweredAt = DateTime.UtcNow;
            }
            else
            {
                _db.OnboardingAnswers.Add(new OnboardingAnswer
                {
                    UserId = userId,
                    QuestionId = incoming.QuestionId,
                    AnswerText = incoming.AnswerText,
                    AnsweredAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task SavePredictionAsync(string userId, PredictionResult prediction)
    {
        // Repositories may call DbContext directly — this is the correct layer for DB writes
        _db.MLPredictions.Add(new MLPrediction
        {
            UserId = userId,
            PrimaryRoleName = prediction.PrimaryRoleName,
            Confidence = prediction.PrimaryConfidence,
            TopRolesJson = prediction.TopRolesJson,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}