using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.Onboarding.Models;

namespace MentraAI.API.Modules.Onboarding.Repositories;

public interface IOnboardingRepository
{
    Task<List<OnboardingQuestion>> GetAllActiveQuestionsAsync();
    Task<List<OnboardingQuestion>> GetQuestionsByIdsAsync(List<int> ids);
    Task<List<OnboardingAnswer>> GetAnswersByUserIdAsync(string userId);
    Task<int> GetAnswerCountByUserIdAsync(string userId);
    Task UpsertAnswersAsync(string userId, List<OnboardingAnswer> answers);

    // Onboarding module owns saving the prediction it triggered.
    Task SavePredictionAsync(string userId, PredictionResult prediction);
}