using System.Text.Json;
using System.Text.Json.Serialization;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;

namespace MentraAI.API.Modules.Quizzes.Services;

public class QuizScoringService : IQuizScoringService
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

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

        // Build lookup: questionId -> correctAnswer (case-insensitive key)
        var correctAnswerMap = stored.ToDictionary(
            q => q.Id,
            q => q.CorrectAnswer,
            StringComparer.OrdinalIgnoreCase);

        int correct = 0;

        foreach (var answer in userAnswers)
        {
            // Unknown questionId counts as wrong — never throw
            if (!correctAnswerMap.TryGetValue(answer.QuestionId, out var correctAnswer))
                continue;

            if (string.Equals(answer.Answer, correctAnswer, StringComparison.OrdinalIgnoreCase))
                correct++;
        }

        int     total    = stored.Count;
        decimal score    = Math.Round((decimal)correct / total * 100, 2);
        // Passing threshold is 50% per blueprint
        bool    isPassed = score >= 50.00m;

        return new QuizScoreResult(correct, total, score, isPassed);
    }

    // Internal deserialization type — correct_answer included, never leaves this class
    private class StoredQuestion
    {
        [JsonPropertyName("id")]             public string Id            { get; set; } = string.Empty;
        [JsonPropertyName("text")]           public string Text          { get; set; } = string.Empty;
        [JsonPropertyName("options")]        public List<string> Options { get; set; } = new();
        [JsonPropertyName("correct_answer")] public string CorrectAnswer { get; set; } = string.Empty;
    }
}
