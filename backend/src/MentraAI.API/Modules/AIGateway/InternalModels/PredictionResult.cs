namespace MentraAI.API.Modules.AIGateway.InternalModels;

// What AIGateway returns to OnboardingService after a successful prediction call.
// Other modules (CareerTracks) also read this to build PredictionResponse DTOs.
public class PredictionResult
{
    public string PrimaryRoleName { get; set; } = string.Empty;
    public decimal PrimaryConfidence { get; set; }

    // Stored as JSON string in MLPredictions.TopRolesJson column.
    // Shape: [{"name":"Backend Developer","confidence":0.87}, ...]
    public string TopRolesJson { get; set; } = "[]";
}