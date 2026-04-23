using System;
using MentraAI.API.Modules.Auth.Models;

namespace MentraAI.API.Modules.Users.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Background { get; set; }
        public string? CurrentSkillsJson { get; set; }
        public string? InterestsJson { get; set; }
        public int? WeeklyHours { get; set; }
        public string? CareerGoals { get; set; }
        public bool IsOnboarded { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
