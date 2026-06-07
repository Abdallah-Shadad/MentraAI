using System.Collections.Generic;
using System.Text.Json;
using MentraAI.API.Modules.Onboarding.DTOs.Requests;
using Xunit;

namespace MentraAI.Tests.Modules.Onboarding.DTOs.Requests;

public class SubmitAnswersRequestValidatorTests
{
    private readonly SubmitAnswersRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidPayloadWithMixedTypes_PassesValidation()
    {
        // Arrange
        // Scenario: Age is a number (25), Education is a string ("bachelors"),
        // Employment is a string ("employed"), Current Skills is an array, Future Skills is an array.
        var json = """
        {
            "Answers": [
                { "QuestionId": 1, "AnswerText": 25 },
                { "QuestionId": 2, "AnswerText": "bachelors" },
                { "QuestionId": 5, "AnswerText": "employed" },
                { "QuestionId": 10, "AnswerText": ["C#", "dotnet"] },
                { "QuestionId": 11, "AnswerText": ["AI", "Cloud"] }
            ]
        }
        """;

        var request = JsonSerializer.Deserialize<SubmitAnswersRequest>(json);
        Assert.NotNull(request);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid, string.Join(", ", result.Errors));
    }

    [Fact]
    public void Validate_EmptySkillsArray_FailsValidation()
    {
        // Arrange
        // Current Skills (10) is an empty JSON array
        var json = """
        {
            "Answers": [
                { "QuestionId": 1, "AnswerText": 25 },
                { "QuestionId": 2, "AnswerText": "bachelors" },
                { "QuestionId": 5, "AnswerText": "employed" },
                { "QuestionId": 10, "AnswerText": [] },
                { "QuestionId": 11, "AnswerText": ["AI", "Cloud"] }
            ]
        }
        """;

        var request = JsonSerializer.Deserialize<SubmitAnswersRequest>(json);
        Assert.NotNull(request);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("AnswerText") && e.ErrorMessage.Contains("Answer text cannot be empty or an empty array []"));
    }

    [Fact]
    public void Validate_MissingRequiredQuestion_FailsValidation()
    {
        // Arrange
        // Missing Future Skills (11)
        var json = """
        {
            "Answers": [
                { "QuestionId": 1, "AnswerText": 25 },
                { "QuestionId": 2, "AnswerText": "bachelors" },
                { "QuestionId": 5, "AnswerText": "employed" },
                { "QuestionId": 10, "AnswerText": ["C#"] }
            ]
        }
        """;

        var request = JsonSerializer.Deserialize<SubmitAnswersRequest>(json);
        Assert.NotNull(request);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("You must answer all required fields"));
    }
}
