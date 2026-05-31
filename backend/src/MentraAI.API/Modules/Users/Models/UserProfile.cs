using System;
using MentraAI.API.Modules.Auth.Models;

namespace MentraAI.API.Modules.Users.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // --- new questions based on ai contract ---
        public string? Age { get; set; }
        public string? EdLevel { get; set; }
        public double? YearsCode { get; set; }
        public double? WorkExp { get; set; }
        public string? Employment { get; set; }
        public string? RemoteWork { get; set; }
        public string? Industry { get; set; }
        public string? OrgSize { get; set; }
        public string? AISelect { get; set; }
        public string? CurrentSkillsJson { get; set; }
        public string? FutureSkillsJson { get; set; }
        // ------------------------------------

        public bool IsOnboarded { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}