using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;

namespace MentraAI.API.Modules.AIGateway.Validators;

public static class RoadmapAIResponseValidator
{
    public static void Validate(RoadmapAIResponse r)
    {
        if (r.Signal != "201_Created")
            throw new AIValidationException($"Unexpected AI signal: {r.Signal}");

        if (r.Roadmap?.Data?.Curriculum?.Stages is null or { Count: 0 })
            throw new AIValidationException("Roadmap contains no stages");

        if (r.Roadmap.Data.TotalWeeks <= 0)
            throw new AIValidationException("total_weeks must be positive");

        for (int i = 0; i < r.Roadmap.Data.Curriculum.Stages.Count; i++)
        {
            var s = r.Roadmap.Data.Curriculum.Stages[i];
            if (string.IsNullOrWhiteSpace(s.Id))
                throw new AIValidationException($"Stage at index {i} has no id");
            if (string.IsNullOrWhiteSpace(s.Name))
                throw new AIValidationException($"Stage {s.Id} has no name");
            if (s.Topics is null or { Count: 0 })
                throw new AIValidationException($"Stage {s.Id} has no topics");
        }
    }
}