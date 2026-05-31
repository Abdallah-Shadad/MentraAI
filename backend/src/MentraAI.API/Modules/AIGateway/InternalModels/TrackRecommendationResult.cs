namespace MentraAI.API.Modules.AIGateway.InternalModels;

// Internal model returned from AIGatewayService to CareerTrackService.
// No JSON attributes — this is pure C# passed between layers.
public class TrackRecommendationResult
{
    public string UserSummary { get; set; } = string.Empty;
    public List<TrackMatch> RecommendedTracks { get; set; } = new();
    public string PrimaryRecommendation { get; set; } = string.Empty;
    public int ProfileCompleteness { get; set; }
    public List<string> MissingInfoSuggestions { get; set; } = new();
}

public class TrackMatch
{
    public string TrackName { get; set; } = string.Empty;
    public int FitScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> SkillOverlap { get; set; } = new();
    public List<string> SkillsToLearn { get; set; } = new();
    public int EstimatedTransitionWeeks { get; set; }
}

