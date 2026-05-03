using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Requests;

public class RoadmapAIRequest
{
    [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("career_track")] public string CareerTrack { get; set; } = string.Empty;
    [JsonPropertyName("weekly_hours")] public int WeeklyHours { get; set; }
    [JsonPropertyName("is_stage_progression")] public bool IsStageProgression { get; set; }

    // Only when is_stage_progression = false (roadmap overview):
    [JsonPropertyName("user_background")] public string? UserBackground { get; set; }
    [JsonPropertyName("current_skills")] public List<string>? CurrentSkills { get; set; }

    // Only when is_stage_progression = true (stage resources):
    [JsonPropertyName("current_stage_index")] public int? CurrentStageIndex { get; set; }
    [JsonPropertyName("curriculum")] public JsonElement? Curriculum { get; set; } // opaque pass-through
    [JsonPropertyName("difficulty_level")] public string? DifficultyLevel { get; set; }
    [JsonPropertyName("skill_gaps")] public List<string>? SkillGaps { get; set; }
    [JsonPropertyName("learner_progress")] public JsonElement? LearnerProgress { get; set; } // opaque pass-through
}