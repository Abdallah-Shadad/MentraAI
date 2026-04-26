namespace MentraAI.API.Modules.AIGateway.InternalModels;

// Placeholder — full implementation in Phase 5 (Quizzes module)
public class QuizGenerationResult
{
    // Full questions + correct answers — stored in QuizAttempts.QuestionsDataJson
    // NEVER sent to frontend
    public string QuestionsDataJson { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    // Display-only questions — no correct_answer field
    public List<QuizQuestionDisplay> Questions { get; set; } = new();
}

public class QuizQuestionDisplay
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    // CorrectAnswer intentionally absent — never exposed to frontend
}