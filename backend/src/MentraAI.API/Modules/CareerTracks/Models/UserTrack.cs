using MentraAI.API.Modules.Auth.Models;

namespace MentraAI.API.Modules.CareerTracks.Models;

public class UserTrack
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CareerTrackId { get; set; }
    public string SelectionType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public CareerTrack CareerTrack { get; set; } = null!;
}