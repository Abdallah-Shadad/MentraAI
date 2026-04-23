using MentraAI.API.Modules.Roadmaps.Models;

namespace MentraAI.API.Modules.StageProgress.Models;

public class UserStageProgress
{
    //public static class StageStatus
    //{
    //    public const string LOCKED = "LOCKED";
    //    public const string ACTIVE = "ACTIVE";
    //    public const string COMPLETED = "COMPLETED";
    //}

    public Guid Id { get; set; } = Guid.NewGuid();
    public int RoadmapId { get; set; }
    public int StageIndex { get; set; }
    public string AiStageId { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public string Status { get; set; } = "LOCKED";
    public string? ResourcesDataJson { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Roadmap Roadmap { get; set; } = null!;
    public ICollection<Quizzes.Models.QuizAttempt> QuizAttempts { get; set; }
        = new List<Quizzes.Models.QuizAttempt>();
}