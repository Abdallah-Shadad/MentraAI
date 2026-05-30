namespace MentraAI.API.Modules.Users.DTOs.Responses;

public class UserProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // --- new fields ---
    public string? Age { get; set; }
    public string? EdLevel { get; set; }
    public double? YearsCode { get; set; }
    public double? WorkExp { get; set; }
    public string? Employment { get; set; }
    public string? RemoteWork { get; set; }
    public string? Industry { get; set; }
    public string? OrgSize { get; set; }
    public string? AISelect { get; set; }
    public List<string> CurrentSkills { get; set; } = new();
    public List<string> FutureSkills { get; set; } = new();
    // ----------------------

    public bool IsOnboarded { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}