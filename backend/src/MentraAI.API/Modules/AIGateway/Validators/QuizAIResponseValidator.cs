using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;

namespace MentraAI.API.Modules.AIGateway.Validators;

public static class QuizAIResponseValidator
{
    public static void Validate(QuizAIResponse r)
    {
        if (r.Signal != "201_Created")
            throw new AIValidationException($"Unexpected signal from AI: {r.Signal}");

        if (r.Quiz is null)
            throw new AIValidationException("Quiz payload is null.");

        if (r.Quiz.Questions is null || r.Quiz.Questions.Count == 0)
            throw new AIValidationException("Quiz contains no questions.");

        foreach (var q in r.Quiz.Questions)
        {
            if (string.IsNullOrWhiteSpace(q.QuestionId))
                throw new AIValidationException("A question has no question_id.");

            if (string.IsNullOrWhiteSpace(q.QuestionText))
                throw new AIValidationException($"Question {q.QuestionId} has no question_text.");

            if (q.Choices is null || q.Choices.Count != 4)
                throw new AIValidationException(
                    $"Question {q.QuestionId} must have exactly 4 choices.");

            if (string.IsNullOrWhiteSpace(q.CorrectAnswer))
                throw new AIValidationException($"Question {q.QuestionId} has no correct_answer.");

            // correct_answer must be one of the choice labels (A, B, C, D)
            var validLabels = q.Choices
                .Select(c => c.Label)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!validLabels.Contains(q.CorrectAnswer))
                throw new AIValidationException(
                    $"Question {q.QuestionId}: correct_answer '{q.CorrectAnswer}' " +
                    $"is not a valid choice label.");

            // Exactly one choice must be marked is_correct
            var correctChoices = q.Choices.Count(c => c.IsCorrect);
            if (correctChoices != 1)
                throw new AIValidationException(
                    $"Question {q.QuestionId}: expected exactly 1 is_correct=true, " +
                    $"found {correctChoices}.");
        }
    }
}