using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Requests;

public class QuizCreateAIRequest
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("career_track")]
    public string CareerTrack { get; set; } = string.Empty;

    [JsonPropertyName("stage_id")]
    public string AiStageId { get; set; } = string.Empty;

    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; } = new();

    [JsonPropertyName("stage_name")]
    public string? StageName { get; set; }

    [JsonPropertyName("difficulty_level")]
    public string? DifficultyLevel { get; set; }

    [JsonPropertyName("learning_objectives")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? LearningObjectives { get; set; }
}