namespace MentraAI.API.Modules.CareerTracks.DTOs.Responses;

public class PredictionResponse
{
    public RoleItem PrimaryRole { get; set; } = null!;
    public List<RoleItem> TopRoles { get; set; } = new();
    public DateTime PredictedAt { get; set; }
}

public class RoleItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}