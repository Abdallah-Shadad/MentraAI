using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;

namespace MentraAI.API.Modules.AIGateway.Validators;

public static class TrackRecommendAIResponseValidator
{
    public static void Validate(TrackRecommendAIResponse r)
    {
        if (r.Signal != "201_Created")
            throw new AIValidationException(
                $"Unexpected signal from track recommender: {r.Signal}");

        if (r.Recommendations is null)
            throw new AIValidationException("Recommendations payload is null.");

        // Graph-level error (LLM or graph initialization failure)
        if (r.Recommendations.Status == "error")
            throw new AIValidationException(
                $"Track recommender graph error: {r.Recommendations.Error ?? "unknown"}");

        var output = r.Recommendations.Data?.Recommendations;

        if (output is null)
            throw new AIValidationException(
                "recommendations.data.recommendations is null.");

        if (output.RecommendedTracks is null || output.RecommendedTracks.Count == 0)
            throw new AIValidationException("No recommended tracks returned.");

        if (output.RecommendedTracks.Count > 5)
            throw new AIValidationException(
                $"Expected 3-5 recommended tracks, got {output.RecommendedTracks.Count}.");

        foreach (var track in output.RecommendedTracks)
        {
            if (string.IsNullOrWhiteSpace(track.TrackName))
                throw new AIValidationException("A recommended track has no track_name.");

            if (track.FitScore < 0 || track.FitScore > 100)
                throw new AIValidationException(
                    $"Track '{track.TrackName}' has invalid fit_score: {track.FitScore}.");
        }
    }
}
