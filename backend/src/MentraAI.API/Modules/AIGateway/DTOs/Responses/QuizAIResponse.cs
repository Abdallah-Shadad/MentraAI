using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Responses;

public class QuizAIResponse
{
    [JsonPropertyName("signal")] public string       Signal { get; set; } = string.Empty;
    [JsonPropertyName("quiz")]   public QuizPayload? Quiz   { get; set; }
}

public class QuizPayload
{
    [JsonPropertyName("questions")] public List<AIQuizQuestion> Questions { get; set; } = new();
}

public class AIQuizQuestion
{
    [JsonPropertyName("id")]             public string       Id            { get; set; } = string.Empty;
    [JsonPropertyName("text")]           public string       Text          { get; set; } = string.Empty;
    [JsonPropertyName("options")]        public List<string> Options       { get; set; } = new();
    [JsonPropertyName("correct_answer")] public string       CorrectAnswer { get; set; } = string.Empty;
}