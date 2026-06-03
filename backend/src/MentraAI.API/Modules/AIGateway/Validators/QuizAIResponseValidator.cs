using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MentraAI.API.Modules.AIGateway.Validators;

public static class QuizAIResponseValidator
{
    public static void Validate(QuizAIResponse r)
    {
        if (r == null)
            throw new AIValidationException("AI response is null.");

        if (!string.Equals(r.Signal, "201_Created", StringComparison.OrdinalIgnoreCase) && 
            !string.Equals(r.Signal, "success", StringComparison.OrdinalIgnoreCase))
        {
            throw new AIValidationException($"Unexpected signal from AI: {r.Signal}");
        }

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

            if (q.Choices is null || q.Choices.Count < 2)
                throw new AIValidationException(
                    $"Question {q.QuestionId} must have at least 2 choices, found {q.Choices?.Count ?? 0}.");

            // Resilient/Auto-healing step 1: If correct_answer is missing, but exactly one choice is marked correct:
            if (string.IsNullOrWhiteSpace(q.CorrectAnswer))
            {
                var correctChoice = q.Choices.FirstOrDefault(c => c.IsCorrect);
                if (correctChoice != null)
                {
                    q.CorrectAnswer = correctChoice.Label;
                }
            }

            // Resilient/Auto-healing step 2: If correct_answer does not match a label, check if it matches choice text
            var validLabels = q.Choices.Select(c => c.Label).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(q.CorrectAnswer) && !validLabels.Contains(q.CorrectAnswer))
            {
                var matchingChoice = q.Choices.FirstOrDefault(c => string.Equals(c.Text, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
                if (matchingChoice != null)
                {
                    q.CorrectAnswer = matchingChoice.Label;
                }
            }

            if (string.IsNullOrWhiteSpace(q.CorrectAnswer))
                throw new AIValidationException($"Question {q.QuestionId} has no correct_answer.");

            validLabels = q.Choices.Select(c => c.Label).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!validLabels.Contains(q.CorrectAnswer))
                throw new AIValidationException(
                    $"Question {q.QuestionId}: correct_answer '{q.CorrectAnswer}' is not a valid choice label.");

            // Resilient/Auto-healing step 3: Force the choice matching correct_answer to be is_correct = true, others false
            foreach (var choice in q.Choices)
            {
                choice.IsCorrect = string.Equals(choice.Label, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            }

            // Double check
            var correctChoices = q.Choices.Count(c => c.IsCorrect);
            if (correctChoices != 1)
                throw new AIValidationException(
                    $"Question {q.QuestionId}: expected exactly 1 is_correct=true, found {correctChoices}.");
        }
    }
}