using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Requests;

// Sent to: POST /api/v1/quiz/adaptation_stage
// Triggered when score < passing_score after quiz submission.
// failed_questions are derived from stored QuestionsDataJson + UserAnswersDataJson.
public class AdaptationAIRequest
{
    [JsonPropertyName("user_id")]            public string              UserId             { get; set; } = string.Empty;
    [JsonPropertyName("career_track")]       public string              CareerTrack        { get; set; } = string.Empty;
    [JsonPropertyName("stage_id")]           public string              StageId            { get; set; } = string.Empty;
    [JsonPropertyName("stage_name")]         public string              StageName          { get; set; } = string.Empty;
    [JsonPropertyName("score")]              public decimal             Score              { get; set; }
    [JsonPropertyName("difficulty_level")]   public string              DifficultyLevel    { get; set; } = string.Empty;
    [JsonPropertyName("learning_objectives")] public List<string>       LearningObjectives { get; set; } = new();
    [JsonPropertyName("failed_questions")]   public List<FailedQuestion> FailedQuestions   { get; set; } = new();
}

public class FailedQuestion
{
    [JsonPropertyName("question")]       public string Question      { get; set; } = string.Empty;
    [JsonPropertyName("user_answer")]    public string UserAnswer    { get; set; } = string.Empty;
    [JsonPropertyName("correct_answer")] public string CorrectAnswer { get; set; } = string.Empty;
}