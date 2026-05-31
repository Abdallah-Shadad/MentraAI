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

    // Stored JSON now uses question_id and choices (new AI contract)
    private const string FourQuestionJson = """
        [
            {
                "question_id": "q1", "question_text": "Q1",
                "choices": [
                    {"label":"A","text":"opt1","is_correct":false},
                    {"label":"B","text":"opt2","is_correct":false},
                    {"label":"C","text":"opt3","is_correct":false},
                    {"label":"D","text":"opt4","is_correct":true}
                ],
                "correct_answer": "D"
            },
            {
                "question_id": "q2", "question_text": "Q2",
                "choices": [
                    {"label":"A","text":"opt1","is_correct":true},
                    {"label":"B","text":"opt2","is_correct":false},
                    {"label":"C","text":"opt3","is_correct":false},
                    {"label":"D","text":"opt4","is_correct":false}
                ],
                "correct_answer": "A"
            },
            {
                "question_id": "q3", "question_text": "Q3",
                "choices": [
                    {"label":"A","text":"opt1","is_correct":false},
                    {"label":"B","text":"opt2","is_correct":true},
                    {"label":"C","text":"opt3","is_correct":false},
                    {"label":"D","text":"opt4","is_correct":false}
                ],
                "correct_answer": "B"
            },
            {
                "question_id": "q4", "question_text": "Q4",
                "choices": [
                    {"label":"A","text":"opt1","is_correct":false},
                    {"label":"B","text":"opt2","is_correct":false},
                    {"label":"C","text":"opt3","is_correct":true},
                    {"label":"D","text":"opt4","is_correct":false}
                ],
                "correct_answer": "C"
            }
        ]
        """;

    [Fact]
    public void Score_AllCorrectLabels_Returns100PercentAndPassed()
    {
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "q1", Answer = "D" },
            new() { QuestionId = "q2", Answer = "A" },
            new() { QuestionId = "q3", Answer = "B" },
            new() { QuestionId = "q4", Answer = "C" }
        };

        var result = _sut.Score(FourQuestionJson, userAnswers);

        Assert.Equal(4, result.CorrectAnswers);
        Assert.Equal(4, result.TotalQuestions);
        Assert.Equal(100.00m, result.Score);
        Assert.True(result.IsPassed);
    }

    [Fact]
    public void Score_70PercentCorrect_IsPassed()
    {
        // 3 out of 4 = 75% → PASS (threshold is 70%)
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "q1", Answer = "D" },
            new() { QuestionId = "q2", Answer = "A" },
            new() { QuestionId = "q3", Answer = "B" },
            new() { QuestionId = "q4", Answer = "A" } // WRONG
        };

        var result = _sut.Score(FourQuestionJson, userAnswers);

        Assert.Equal(3, result.CorrectAnswers);
        Assert.Equal(75.00m, result.Score);
        Assert.True(result.IsPassed);
    }

    [Fact]
    public void Score_50PercentCorrect_IsNotPassed()
    {
        // 2 out of 4 = 50% → FAIL (threshold is 70%, was 50% — this tests the fix)
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "q1", Answer = "D" },
            new() { QuestionId = "q2", Answer = "A" },
            new() { QuestionId = "q3", Answer = "A" }, // WRONG
            new() { QuestionId = "q4", Answer = "A" }  // WRONG
        };

        var result = _sut.Score(FourQuestionJson, userAnswers);

        Assert.Equal(2, result.CorrectAnswers);
        Assert.Equal(50.00m, result.Score);
        Assert.False(result.IsPassed); // 50% < 70% → FAIL
    }

    [Fact]
    public void Score_CaseInsensitiveLabel_CountsAsCorrect()
    {
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "q1", Answer = "d" },  // lowercase label
            new() { QuestionId = "q2", Answer = "a" },
            new() { QuestionId = "q3", Answer = "b" },
            new() { QuestionId = "q4", Answer = "c" }
        };

        var result = _sut.Score(FourQuestionJson, userAnswers);

        Assert.Equal(4, result.CorrectAnswers);
        Assert.Equal(100.00m, result.Score);
        Assert.True(result.IsPassed);
    }

    [Fact]
    public void Score_UnknownQuestionId_CountsAsWrongButDoesNotThrow()
    {
        var userAnswers = new List<QuizAnswerItem>
        {
            new() { QuestionId = "UNKNOWN", Answer = "A" }
        };

        var result = _sut.Score(FourQuestionJson, userAnswers);

        Assert.Equal(0, result.CorrectAnswers);
        Assert.Equal(0.00m, result.Score);
        Assert.False(result.IsPassed);
    }

    [Fact]
    public void Score_MalformedJson_ThrowsAppExceptionWithInternalError()
    {
        var malformedJson = "{ not array }";
        var userAnswers = new List<QuizAnswerItem>();

        var ex = Assert.Throws<AppException>(() => _sut.Score(malformedJson, userAnswers));
        Assert.Equal("INTERNAL_ERROR", ex.ErrorCode);
        Assert.Equal(500, ex.StatusCode);
    }
}