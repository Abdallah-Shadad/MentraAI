namespace MentraAI.API.Modules.Users.DTOs.Responses;

public class UserProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Background { get; set; }
    public List<string> CurrentSkills { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public int? WeeklyHours { get; set; }
    public string? CareerGoals { get; set; }
    public bool IsOnboarded { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}