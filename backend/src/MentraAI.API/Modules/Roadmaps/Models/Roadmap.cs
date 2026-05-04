using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.StageProgress.Models;

namespace MentraAI.API.Modules.Roadmaps.Models;

public class Roadmap
{
    public int Id { get; set; }
    public int UserTrackId { get; set; }
    public int VersionNumber { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string TriggerType { get; set; } = "INITIAL";
    public string RoadmapDataJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserTrack UserTrack { get; set; } = null!;
    public ICollection<UserStageProgress> Stages { get; set; } = new List<UserStageProgress>();
}