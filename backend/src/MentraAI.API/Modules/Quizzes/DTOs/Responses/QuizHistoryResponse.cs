namespace MentraAI.API.Modules.Quizzes.DTOs.Responses;

public class QuizHistoryResponse
{
    public List<QuizAttemptSummary> Attempts { get; set; } = new();
}

public class QuizAttemptSummary
{
    public Guid      QuizId        { get; set; }
    public int       AttemptNumber { get; set; }
    public decimal   Score         { get; set; }
    public bool      IsPassed      { get; set; }
    public bool      IsSubmitted   { get; set; }
    public DateTime  GeneratedAt   { get; set; }
    public DateTime? SubmittedAt   { get; set; }
}
