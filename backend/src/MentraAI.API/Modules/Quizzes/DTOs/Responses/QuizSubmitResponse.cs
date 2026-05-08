namespace MentraAI.API.Modules.Quizzes.DTOs.Responses;

public class QuizSubmitResponse
{
    public Guid           QuizId          { get; set; }
    public decimal        Score           { get; set; }
    public int            CorrectAnswers  { get; set; }
    public int            TotalQuestions  { get; set; }
    public bool           IsPassed        { get; set; }
    public DateTime       SubmittedAt     { get; set; }
    // null when: last stage passed, or quiz failed (adaptation triggered or not)
    public NextStageInfo? NextStage       { get; set; }
    // true if adaptation ran successfully after a fail
    public bool           RoadmapAdapted  { get; set; }
}

public class NextStageInfo
{
    public Guid   StageProgressId { get; set; }
    public string StageName       { get; set; } = string.Empty;
    public int    StageIndex      { get; set; }
}
