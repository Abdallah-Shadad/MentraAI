namespace MentraAI.API.Modules.CareerTracks.Models;

public class CareerTrack
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserTrack> UserTracks { get; set; } = new List<UserTrack>();
}