namespace MentraAI.API.Modules.AIGateway.InternalModels;

// Internal model returned from AIGatewayService.GetAdaptedRoadmapAsync to QuizService.
// Contains the raw remedial resources JSON to store in UserStageProgress.ResourcesDataJson
// and the human-readable summary for logging/display.
public class AdaptationResult
{
    // The full AI response JSON — stored as-is in UserStageProgress.ResourcesDataJson.
    // Frontend reads this via GET /stages/{id}/resources just like normal stage resources.
    public string RemediationResourcesJson { get; set; } = string.Empty;

    // Human-readable summary of what the learner struggled with
    public string Summary { get; set; } = string.Empty;

    // Topics the learner needs to review
    public List<string> StrugglingTopics { get; set; } = new();
}
