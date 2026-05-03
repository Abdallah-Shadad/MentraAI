using System;
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

    // Nullable weekly hours so callers can do `roadmap.UserTrack.WeeklyHours ?? 10`
    public int? WeeklyHours { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public CareerTrack CareerTrack { get; set; } = null!;
}