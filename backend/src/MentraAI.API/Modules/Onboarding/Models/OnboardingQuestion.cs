using System;

namespace MentraAI.API.Modules.Onboarding.Models
{
    public class OnboardingQuestion
    {
        public int Id { get; set; }
        public string QuestionKey { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string? OptionsJson { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OnboardingAnswer> Answers { get; set; } = new List<OnboardingAnswer>();
    }
}
