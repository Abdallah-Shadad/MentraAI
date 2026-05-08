using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Requests;

public class QuizCreateAIRequest
{
    [JsonPropertyName("user_id")]          public string UserId          { get; set; } = string.Empty;
    [JsonPropertyName("career_track")]     public string CareerTrack     { get; set; } = string.Empty;
    [JsonPropertyName("stage_id")]         public string AiStageId       { get; set; } = string.Empty;
    [JsonPropertyName("stage_name")]       public string StageName       { get; set; } = string.Empty;
    [JsonPropertyName("difficulty_level")] public string DifficultyLevel { get; set; } = string.Empty;
}