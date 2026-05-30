using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Requests;

// Sent to: POST /api/v1/tracks/recommend
// All field names match AI contract exactly (PascalCase for categorical, snake_case for skills).
// All profile fields optional — AI handles missing fields gracefully.
public class TrackRecommendAIRequest
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("profile")]
    public TrackRecommendProfile Profile { get; set; } = new();
}

public class TrackRecommendProfile
{
    // Categorical fields — accepted values defined in AI contract
    [JsonPropertyName("Age")]        public string? Age        { get; set; }
    [JsonPropertyName("EdLevel")]    public string? EdLevel    { get; set; }
    [JsonPropertyName("Employment")] public string? Employment { get; set; }
    [JsonPropertyName("RemoteWork")] public string? RemoteWork { get; set; }
    [JsonPropertyName("Industry")]   public string? Industry   { get; set; }
    [JsonPropertyName("OrgSize")]    public string? OrgSize    { get; set; }
    [JsonPropertyName("AISelect")]   public string? AISelect   { get; set; }

    // Numeric
    [JsonPropertyName("YearsCode")] public double? YearsCode { get; set; }
    [JsonPropertyName("WorkExp")]   public double? WorkExp   { get; set; }

    // Skills — lowercase canonical vocabulary per AI contract
    [JsonPropertyName("current_skills")] public List<string>? CurrentSkills { get; set; }
    [JsonPropertyName("future_skills")]  public List<string>? FutureSkills  { get; set; }
}
