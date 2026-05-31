using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Requests;

// Sent to: POST /api/v1/machine_model/predict
// All field names use snake_case
// Temporary contract based on onboarding schema.
// todo: AI team to finalize contract, we may need to adjust fields and types here.
public class PredictAIRequest
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("user_background")]
    public string Background { get; set; } = string.Empty;

    [JsonPropertyName("current_skills")]
    public List<string> Skills { get; set; } = new();

    [JsonPropertyName("interests")]
    public List<string> Interests { get; set; } = new();

    [JsonPropertyName("weekly_hours")]
    public int WeeklyHours { get; set; }

    [JsonPropertyName("career_goals")]
    public string CareerGoals { get; set; } = string.Empty;
}