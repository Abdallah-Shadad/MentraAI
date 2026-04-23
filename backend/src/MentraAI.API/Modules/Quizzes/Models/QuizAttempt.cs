using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.StageProgress.Models;

namespace MentraAI.API.Modules.Quizzes.Models;

public class QuizAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StageProgressId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public bool IsSubmitted { get; set; } = false;
    public string QuestionsDataJson { get; set; } = string.Empty;
    public string? UserAnswersDataJson { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; } = 0;
    public decimal Score { get; set; } = 0;
    public bool IsPassed { get; set; } = false;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    public UserStageProgress StageProgress { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}