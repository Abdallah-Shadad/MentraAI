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

    public QuizScoreResult Score(string questionsDataJson, List<QuizAnswerItem> userAnswers, decimal passingScore)
    {
        if (passingScore < 0 || passingScore > 100)
        {
            passingScore = 70m;
        }
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

        var correctAnswerMap = stored.ToDictionary(
            q => q.QuestionId,
            q => q.CorrectAnswer,
            StringComparer.OrdinalIgnoreCase);

        int correct = 0;

        foreach (var answer in userAnswers)
        {
            if (!correctAnswerMap.TryGetValue(answer.QuestionId, out var correctAnswer))
                continue;

            if (string.Equals(answer.Answer, correctAnswer, StringComparison.OrdinalIgnoreCase))
                correct++;
        }

        int total = stored.Count;
        decimal score = Math.Round((decimal)correct / total * 100, 2);

        // Determine pass/fail based on the provided passing score
        bool isPassed = score >= passingScore;

        return new QuizScoreResult(correct, total, score, isPassed);
    }

    public QuizScoreResult Score(string questionsDataJson, List<QuizAnswerItem> userAnswers)
    {
        return Score(questionsDataJson, userAnswers, 70.00m);
    }

    // This method is no longer needed since we now require the passing score to be explicitly provided.
    //private static decimal NormalizePassingScoreOrDefault(int? passingScore)
    //{
    //    // AI contract uses integers like 70/80. If missing or invalid, fallback to 70.
    //    var value = passingScore ?? 70;
    //    if (value < 0 || value > 100) value = 70;
    //    return value;
    //}

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