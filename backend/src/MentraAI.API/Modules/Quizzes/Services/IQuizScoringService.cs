using MentraAI.API.Modules.Quizzes.DTOs.Requests;
namespace MentraAI.API.Modules.Quizzes.Services;
public interface IQuizScoringService
{
    // Backward-compatible overload
    // Used by old unit tests and any legacy callers
    QuizScoreResult Score(string questionsDataJson, List<QuizAnswerItem> userAnswers);

    // Main overload with dynamic passing score
    QuizScoreResult Score(string questionsDataJson, List<QuizAnswerItem> userAnswers, decimal passingScore);
}
public record QuizScoreResult(int CorrectAnswers, int TotalQuestions, decimal Score, bool IsPassed);