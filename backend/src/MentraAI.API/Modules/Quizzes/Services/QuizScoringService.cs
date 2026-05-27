using System.Text.Json;
using System.Text.Json.Serialization;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;

namespace MentraAI.API.Modules.Quizzes.Services;

public class QuizScoringService : IQuizScoringService
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QuizScoreResult Score(string questionsDataJson, List<QuizAnswerItem> userAnswers)
    {
        // Malformed stored data = corruption, not a user error
        List<StoredQuestion> stored;
        try
        {
            stored = JsonSerializer.Deserialize<List<StoredQuestion>>(questionsDataJson, _json)
                     ?? throw new Exception("Null deserialization result.");
        }
        catch (Exception ex)
        {
            throw new AppException(ErrorCodes.INTERNAL_ERROR,
                $"Quiz data corrupted: {ex.Message}", 500);
        }

        if (stored.Count == 0)
            throw new AppException(ErrorCodes.INTERNAL_ERROR, "Stored quiz has no questions.", 500);

        // Build lookup: questionId -> correctAnswer (label like "B")
        // Stored JSON uses question_id (new AI contract)
        var correctAnswerMap = stored.ToDictionary(
            q => q.QuestionId,
            q => q.CorrectAnswer,
            StringComparer.OrdinalIgnoreCase);

        int correct = 0;

        foreach (var answer in userAnswers)
        {
            // Unknown questionId counts as wrong — never throw
            if (!correctAnswerMap.TryGetValue(answer.QuestionId, out var correctAnswer))
                continue;

            // Frontend submits label "B", stored correct_answer is "B"
            if (string.Equals(answer.Answer, correctAnswer, StringComparison.OrdinalIgnoreCase))
                correct++;
        }

        int total = stored.Count;
        decimal score = Math.Round((decimal)correct / total * 100, 2);
        // FIXED: Passing threshold is 70% (was 50%)
        bool isPassed = score >= 70.00m;

        return new QuizScoreResult(correct, total, score, isPassed);
    }

    // Internal deserialization type matching the NEW AI response structure
    // correct_answer is a LABEL ("B"), never leaves this class
    private class StoredQuestion
    {
        [JsonPropertyName("question_id")] public string QuestionId { get; set; } = string.Empty;
        [JsonPropertyName("question_text")] public string QuestionText { get; set; } = string.Empty;
        [JsonPropertyName("choices")] public List<object> Choices { get; set; } = new();
        [JsonPropertyName("correct_answer")] public string CorrectAnswer { get; set; } = string.Empty;
        // explanation and hints exist in JSON but are not needed for scoring
    }
}