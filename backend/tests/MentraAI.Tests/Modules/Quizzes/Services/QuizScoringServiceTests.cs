using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.Services;

namespace MentraAI.Tests.Modules.Quizzes.Services;

public class QuizScoringServiceTests
{
    private readonly QuizScoringService _sut;

    public QuizScoringServiceTests()
    {
        _sut = new QuizScoringService();
    }

    [Fact]
    public void Score_AllCorrect_Returns100PercentAndPassed()
    {
        // Arrange
        var questionsJson = """
        [
            { "id": "q1", "text": "Q1", "options": ["A", "B"], "correct_answer": "A" },
            { "id": "q2", "text": "Q2", "options": ["C", "D"], "correct_answer": "D" }
        ]
        """;
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "q1", Answer = "A" },
            new() { QuestionId = "q2", Answer = "D" }
        };

        // Act
        var result = _sut.Score(questionsJson, userAnswers);

        // Assert
        Assert.Equal(2, result.CorrectAnswers);
        Assert.Equal(2, result.TotalQuestions);
        Assert.Equal(100.00m, result.Score);
        Assert.True(result.IsPassed);
    }

    [Fact]
    public void Score_HalfCorrect_Returns50PercentAndPassed()
    {
        // Arrange
        var questionsJson = """
        [
            { "id": "q1", "text": "Q1", "options": ["A", "B"], "correct_answer": "A" },
            { "id": "q2", "text": "Q2", "options": ["C", "D"], "correct_answer": "D" }
        ]
        """;
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "q1", Answer = "A" },
            new() { QuestionId = "q2", Answer = "Wrong" }
        };

        // Act
        var result = _sut.Score(questionsJson, userAnswers);

        // Assert
        Assert.Equal(1, result.CorrectAnswers);
        Assert.Equal(2, result.TotalQuestions);
        Assert.Equal(50.00m, result.Score);
        Assert.True(result.IsPassed); // 50% is the passing threshold
    }

    [Fact]
    public void Score_CaseInsensitive_CountsAsCorrect()
    {
        // Arrange
        var questionsJson = """
        [
            { "id": "q1", "text": "Q1", "options": ["Apple", "Banana"], "correct_answer": "Apple" }
        ]
        """;
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "q1", Answer = "aPpLe" }
        };

        // Act
        var result = _sut.Score(questionsJson, userAnswers);

        // Assert
        Assert.Equal(1, result.CorrectAnswers);
        Assert.Equal(100.00m, result.Score);
    }

    [Fact]
    public void Score_UnknownQuestionId_CountsAsWrongButDoesNotThrow()
    {
        // Arrange
        var questionsJson = """
        [
            { "id": "q1", "text": "Q1", "options": ["A", "B"], "correct_answer": "A" }
        ]
        """;
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "UNKNOWN", Answer = "A" }
        };

        // Act
        var result = _sut.Score(questionsJson, userAnswers);

        // Assert
        Assert.Equal(0, result.CorrectAnswers);
        Assert.Equal(0.00m, result.Score);
    }

    [Fact]
    public void Score_MalformedJson_ThrowsAppExceptionWithInternalError()
    {
        // Arrange
        var malformedJson = "{ not array }";
        var userAnswers = new List<QuizAnswerItem>();

        // Act & Assert
        var ex = Assert.Throws<AppException>(() => _sut.Score(malformedJson, userAnswers));
        Assert.Equal("INTERNAL_ERROR", ex.ErrorCode);
        Assert.Equal(500, ex.StatusCode);
    }
}
