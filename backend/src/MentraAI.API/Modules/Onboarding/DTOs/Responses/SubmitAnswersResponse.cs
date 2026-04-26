namespace MentraAI.API.Modules.Onboarding.DTOs.Responses;

public class SubmitAnswersResponse
{
    public bool IsOnboarded { get; set; }
    public PredictionData Prediction { get; set; } = null!;
}

public class PredictionData
{
    public RoleData PrimaryRole { get; set; } = null!;
    public List<RoleData> TopRoles { get; set; } = new();
}

public class RoleData
{
    public string Name { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}