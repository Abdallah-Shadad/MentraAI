using MentraAI.API.Modules.Onboarding.DTOs.Requests;
using MentraAI.API.Modules.Onboarding.DTOs.Responses;

namespace MentraAI.API.Modules.Onboarding.Services;

public interface IOnboardingService
{
    Task<QuestionsListResponse> GetQuestionsAsync();
    Task<OnboardingStatusResponse> GetStatusAsync(string userId);
    Task<SubmitAnswersResponse> SubmitAnswersAsync(string userId, SubmitAnswersRequest request);
}