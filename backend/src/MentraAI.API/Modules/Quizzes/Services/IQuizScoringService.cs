using MentraAI.API.Modules.Quizzes.DTOs.Requests;

namespace MentraAI.API.Modules.Quizzes.Services;

public interface IQuizScoringService
{
    // Pure synchronous — no DB, no AI
    QuizScoreResult Score(string questionsDataJson, List<QuizAnswerItem> userAnswers);
}

public record QuizScoreResult(int CorrectAnswers, int TotalQuestions, decimal Score, bool IsPassed);
