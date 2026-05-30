using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Responses;

// Full deep structure matching AI contract response exactly
public class TrackRecommendAIResponse
{
    [JsonPropertyName("signal")]          public string                Signal          { get; set; } = string.Empty;
    [JsonPropertyName("status")]          public string                Status          { get; set; } = string.Empty;
    [JsonPropertyName("message")]         public string                Message         { get; set; } = string.Empty;
    [JsonPropertyName("recommendations")] public TrackRecommendPayload? Recommendations { get; set; }
    [JsonPropertyName("time_consumed")]   public double                TimeConsumed    { get; set; }
}

public class TrackRecommendPayload
{
    [JsonPropertyName("status")]  public string             Status { get; set; } = string.Empty;
    [JsonPropertyName("mode")]    public string             Mode   { get; set; } = string.Empty;
    [JsonPropertyName("user_id")] public string             UserId { get; set; } = string.Empty;
    [JsonPropertyName("data")]    public TrackRecommendData? Data  { get; set; }
    [JsonPropertyName("error")]   public string?             Error { get; set; }
}

public class TrackRecommendData
{
    [JsonPropertyName("recommendations")]
    public TrackRecommendOutput? Recommendations { get; set; }
}

public class TrackRecommendOutput
{
    [JsonPropertyName("user_summary")]
    public string UserSummary { get; set; } = string.Empty;

    [JsonPropertyName("recommended_tracks")]
    public List<AITrackMatch> RecommendedTracks { get; set; } = new();

    [JsonPropertyName("primary_recommendation")]
    public string PrimaryRecommendation { get; set; } = string.Empty;

    [JsonPropertyName("profile_completeness")]
    public int ProfileCompleteness { get; set; }

    [JsonPropertyName("missing_info_suggestions")]
    public List<string>? MissingInfoSuggestions { get; set; }
}

public class AITrackMatch
{
    [JsonPropertyName("track_name")]
    public string TrackName { get; set; } = string.Empty;

    [JsonPropertyName("fit_score")]
    public int FitScore { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    [JsonPropertyName("skill_overlap")]
    public List<string> SkillOverlap { get; set; } = new();

    [JsonPropertyName("skills_to_learn")]
    public List<string> SkillsToLearn { get; set; } = new();

    [JsonPropertyName("estimated_transition_weeks")]
    public int EstimatedTransitionWeeks { get; set; }
}
