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
            if (string.IsNullOrWhiteSpace(q.Id))
                throw new AIValidationException("A question has no id.");

            if (string.IsNullOrWhiteSpace(q.Text))
                throw new AIValidationException($"Question {q.Id} has no text.");

            if (q.Options is null || q.Options.Count != 4)
                throw new AIValidationException($"Question {q.Id} must have exactly 4 options.");

            if (string.IsNullOrWhiteSpace(q.CorrectAnswer))
                throw new AIValidationException($"Question {q.Id} has no correct_answer.");

            if (!q.Options.Contains(q.CorrectAnswer, StringComparer.OrdinalIgnoreCase))
                throw new AIValidationException(
                    $"Question {q.Id}: correct_answer is not one of the options.");
        }
    }
}
