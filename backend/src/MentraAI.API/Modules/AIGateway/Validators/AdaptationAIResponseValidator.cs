using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;

namespace MentraAI.API.Modules.AIGateway.Validators;

public static class AdaptationAIResponseValidator
{
    public static void Validate(AdaptationAIResponse r)
    {
        if (r.Signal != "201_Created")
            throw new AIValidationException(
                $"Unexpected signal from adaptation engine: {r.Signal}");

        if (r.AdditionalResource is null)
            throw new AIValidationException("Adaptation payload (Additional_Resource) is null.");

        if (r.AdditionalResource.Status == "error")
            throw new AIValidationException(
                $"Adaptation engine error: {r.AdditionalResource.Error ?? "unknown"}");

        if (!r.AdditionalResource.Adapted)
            throw new AIValidationException("Adaptation returned adapted=false unexpectedly.");

        var data = r.AdditionalResource.Data;
        if (data is null)
            throw new AIValidationException("Adaptation data is null.");

        var stages = data.Curriculum?.Stages;
        if (stages is null || stages.Count == 0)
            throw new AIValidationException("Adaptation curriculum contains no stages.");

        // The adapted stage must have at least 2 remedial resources (contract: 2 videos + 2 articles)
        var adaptedStage = stages.FirstOrDefault(s => s.Adapted);
        if (adaptedStage is null)
            throw new AIValidationException("No adapted stage found in adaptation response.");

        if (adaptedStage.Resources is null || adaptedStage.Resources.Count == 0)
            throw new AIValidationException(
                $"Adapted stage '{adaptedStage.Id}' has no remedial resources.");
    }
}