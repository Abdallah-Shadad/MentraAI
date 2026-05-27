using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.Chat.DTOs.Requests;

/// <summary>
/// Internal DTO — what WE send to the AI service.
/// user_id is injected server-side from JWT, never from the client.
/// </summary>
public class ChatAIRequest
{
    [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("conversation_id")] public string ConversationId { get; set; } = string.Empty;
    [JsonPropertyName("query")] public string Query { get; set; } = string.Empty;
    [JsonPropertyName("career_track")] public string? CareerTrack { get; set; }
    [JsonPropertyName("stage")] public string? Stage { get; set; }
    [JsonPropertyName("lesson_id")] public string? LessonId { get; set; }
    [JsonPropertyName("quiz_details")] public string? QuizDetails { get; set; }
    [JsonPropertyName("quiz_score")] public int? QuizScore { get; set; }

}