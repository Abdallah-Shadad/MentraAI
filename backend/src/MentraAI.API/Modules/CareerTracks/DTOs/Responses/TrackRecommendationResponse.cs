namespace MentraAI.API.Modules.CareerTracks.DTOs.Responses;

// Frontend-facing response — safe to expose fully (no sensitive data)
public class TrackRecommendationResponse
{
    public string                   UserSummary            { get; set; } = string.Empty;
    public List<TrackMatchResponse> RecommendedTracks      { get; set; } = new();
    public string                   PrimaryRecommendation  { get; set; } = string.Empty;
    public int                      ProfileCompleteness    { get; set; }
    public List<string>             MissingInfoSuggestions { get; set; } = new();
}

public class TrackMatchResponse
{
    public string       TrackName                { get; set; } = string.Empty;
    public int          FitScore                 { get; set; }
    public string       Reasoning                { get; set; } = string.Empty;
    public List<string> SkillOverlap             { get; set; } = new();
    public List<string> SkillsToLearn            { get; set; } = new();
    public int          EstimatedTransitionWeeks { get; set; }
}
