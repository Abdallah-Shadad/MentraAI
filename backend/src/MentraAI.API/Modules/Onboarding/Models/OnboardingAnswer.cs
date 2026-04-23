using System;
using MentraAI.API.Modules.Auth.Models;

namespace MentraAI.API.Modules.Onboarding.Models
{
    public class OnboardingAnswer
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;
        public OnboardingQuestion Question { get; set; } = null!;
    }
}
