using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;

namespace MentraAI.API.Modules.AIGateway.Validators;

public static class PredictAIResponseValidator
{
    public static void Validate(PredictAIResponse response)
    {
        if (response.PrimaryRole is null)
            throw new AIValidationException("AI prediction response missing primary_role.");

        if (string.IsNullOrWhiteSpace(response.PrimaryRole.Name))
            throw new AIValidationException("AI prediction primary_role.name is empty.");

        if (response.PrimaryRole.Confidence < 0 || response.PrimaryRole.Confidence > 1)
            throw new AIValidationException(
                $"AI prediction confidence out of range: {response.PrimaryRole.Confidence}");

        if (response.TopRoles is null || response.TopRoles.Count == 0)
            throw new AIValidationException("AI prediction response missing top_roles.");
    }
}