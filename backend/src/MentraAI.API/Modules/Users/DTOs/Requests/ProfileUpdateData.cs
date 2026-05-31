namespace MentraAI.API.Modules.Users.DTOs.Requests;

// Internal carrier — not exposed to frontend.
// Onboarding module passes this to IUserService.UpdateProfileFromAnswersAsync.
public class ProfileUpdateData
{
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
}