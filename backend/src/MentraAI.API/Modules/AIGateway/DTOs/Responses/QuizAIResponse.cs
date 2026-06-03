using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Responses;

public class QuizAIResponse
{
    [JsonPropertyName("signal")] public string Signal { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("time_consumed")] public float TimeConsumed { get; set; }
    [JsonPropertyName("quiz")] public QuizPayload? Quiz { get; set; }
}

public class QuizPayload
{
    [JsonPropertyName("stage_id")] public string StageId { get; set; } = string.Empty;
    [JsonPropertyName("topic")] public string Topic { get; set; } = string.Empty;
    [JsonPropertyName("difficulty_level")] public string DifficultyLevel { get; set; } = string.Empty;
    [JsonPropertyName("total_questions")] public int TotalQuestions { get; set; }
    [JsonPropertyName("time_limit_minutes")] public int TimeLimitMinutes { get; set; }
    [JsonPropertyName("passing_score")] public int PassingScore { get; set; }
    [JsonPropertyName("questions")] public List<AIQuizQuestion> Questions { get; set; } = new();
}

public class AIQuizQuestion
{
    [JsonPropertyName("question_id")] public string QuestionId { get; set; } = string.Empty;
    [JsonPropertyName("question_text")] public string QuestionText { get; set; } = string.Empty;
    [JsonPropertyName("question_type")] public string QuestionType { get; set; } = string.Empty;
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; } = string.Empty;
    [JsonPropertyName("bloom_level")] public string BloomLevel { get; set; } = string.Empty;
    [JsonPropertyName("topic")] public string Topic { get; set; } = string.Empty;
    [JsonPropertyName("choices")] public List<AIChoice> Choices { get; set; } = new();
    [JsonPropertyName("correct_answer")] public string CorrectAnswer { get; set; } = string.Empty;
    [JsonPropertyName("explanation")] public string Explanation { get; set; } = string.Empty;
    [JsonPropertyName("hints")] public List<AIHint> Hints { get; set; } = new();
}

public class AIChoice
{
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
    [JsonPropertyName("is_correct")] public bool IsCorrect { get; set; }
}

public class AIHint
{
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
}