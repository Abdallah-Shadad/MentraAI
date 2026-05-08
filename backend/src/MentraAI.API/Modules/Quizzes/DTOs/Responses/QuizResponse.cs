namespace MentraAI.API.Modules.Quizzes.DTOs.Responses;

public class QuizResponse
{
    public Guid                  QuizId          { get; set; }
    public Guid                  StageProgressId { get; set; }
    public int                   AttemptNumber   { get; set; }
    public int                   TotalQuestions  { get; set; }
    public DateTime              GeneratedAt     { get; set; }
    // correct_answer intentionally absent from every item in this list
    public List<QuizQuestionItem> Questions      { get; set; } = new();
}

public class QuizQuestionItem
{
    public string        Id      { get; set; } = string.Empty;
    public string        Text    { get; set; } = string.Empty;
    public List<string>  Options { get; set; } = new();
}
