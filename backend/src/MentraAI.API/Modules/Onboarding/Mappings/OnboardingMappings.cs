using System.Text.Json;
using MentraAI.API.Modules.Onboarding.Models;
using MentraAI.API.Modules.Onboarding.DTOs.Responses;

namespace MentraAI.API.Modules.Onboarding;

public static class OnboardingMappings
{
    // == OnboardingQuestion -> QuestionItem ==
    public static QuestionItem ToQuestionItem(OnboardingQuestion question)
    {
        List<string>? options = null;

        if (!string.IsNullOrWhiteSpace(question.OptionsJson))
        {
            try { options = JsonSerializer.Deserialize<List<string>>(question.OptionsJson); }
            catch { options = null; }
        }

        return new QuestionItem
        {
            QuestionId = question.Id,
            QuestionKey = question.QuestionKey,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType,
            Options = options,
            DisplayOrder = question.DisplayOrder
        };
    }
}