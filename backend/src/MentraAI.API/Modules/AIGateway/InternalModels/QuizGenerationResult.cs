namespace MentraAI.API.Modules.AIGateway.InternalModels;

public class QuizGenerationResult
{
    // Full questions + correct answers — stored in QuizAttempts.QuestionsDataJson
    // NEVER sent to frontend
    public string QuestionsDataJson { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }

    // NEW — from AI response
    public int PassingScore { get; set; }
    public int TimeLimitMinutes { get; set; }

    // Display-only questions — no correct_answer, no explanation, no hints
    public List<QuizQuestionDisplay> Questions { get; set; } = new();
}

public class QuizQuestionDisplay
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<QuizChoiceDisplay> Choices { get; set; } = new();  // CHANGED from List<string>
}

// Safe for frontend — label + display text only, is_correct intentionally absent
public class QuizChoiceDisplay
{
    public string Label { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}