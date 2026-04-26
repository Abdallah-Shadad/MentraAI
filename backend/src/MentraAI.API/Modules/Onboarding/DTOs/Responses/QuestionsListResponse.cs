namespace MentraAI.API.Modules.Onboarding.DTOs.Responses;

public class QuestionsListResponse
{
    public List<QuestionItem> Questions { get; set; } = new();
}

public class QuestionItem
{
    // Integer PK from OnboardingQuestions — sent back in answer submission
    public int QuestionId { get; set; }
    // Machine key used to map answer to AI request field
    public string QuestionKey { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    // TEXT | NUMBER | SINGLESELECT | MULTISELECT
    public string QuestionType { get; set; } = string.Empty;
    // Null for TEXT and NUMBER, array of strings for SELECT types
    public List<string>? Options { get; set; }
    public int DisplayOrder { get; set; }
}