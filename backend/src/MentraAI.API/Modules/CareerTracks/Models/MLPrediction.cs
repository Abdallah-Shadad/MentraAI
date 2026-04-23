using MentraAI.API.Modules.Auth.Models;

namespace MentraAI.API.Modules.CareerTracks.Models;

public class MLPrediction
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PrimaryRoleName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string TopRolesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}