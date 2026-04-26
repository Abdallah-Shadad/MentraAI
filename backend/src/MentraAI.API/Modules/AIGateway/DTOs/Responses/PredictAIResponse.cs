using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Responses;

// Shape returned by POST /api/v1/machine_model/predict
public class PredictAIResponse
{
    [JsonPropertyName("primary_role")]
    public AIRoleItem? PrimaryRole { get; set; }

    [JsonPropertyName("top_roles")]
    public List<AIRoleItem> TopRoles { get; set; } = new();
}

public class AIRoleItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }
}