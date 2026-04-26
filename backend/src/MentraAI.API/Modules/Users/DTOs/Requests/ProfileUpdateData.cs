namespace MentraAI.API.Modules.Users.DTOs.Requests;

// Internal carrier — not exposed to frontend.
// Onboarding module passes this to IUserService.UpdateProfileFromAnswersAsync.
public class ProfileUpdateData
{
    public string? Background { get; set; }
    public int? WeeklyHours { get; set; }
    public string? CurrentSkillsJson { get; set; }
    public string? InterestsJson { get; set; }
    public string? CareerGoals { get; set; }
}