using System.Text.Json;
using System.Text.Json.Serialization;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.Onboarding.DTOs.Responses;
using MentraAI.API.Modules.Onboarding.Models;

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

    // == PredictionResult -> SubmitAnswersResponse ==
    public static SubmitAnswersResponse ToSubmitResponse(
        PredictionResult prediction, bool isOnboarded)
    {
        List<RoleData> topRoles;

        try
        {
            var raw = JsonSerializer.Deserialize<List<RoleRaw>>(prediction.TopRolesJson)
                      ?? new List<RoleRaw>();

            topRoles = raw
                .Select(r => new RoleData { Name = r.Name, Confidence = r.Confidence })
                .ToList();
        }
        catch { topRoles = new List<RoleData>(); }

        return new SubmitAnswersResponse
        {
            IsOnboarded = isOnboarded,
            Prediction = new PredictionData
            {
                PrimaryRole = new RoleData
                {
                    Name = prediction.PrimaryRoleName,
                    Confidence = prediction.PrimaryConfidence
                },
                TopRoles = topRoles
            }
        };
    }

    // Private helper matching AI response JSON shape
    private class RoleRaw
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("confidence")] public decimal Confidence { get; set; }
    }
}