using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Requests;

public class RoadmapAIRequest
{
    [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("career_track")] public string CareerTrack { get; set; } = string.Empty;
    [JsonPropertyName("weekly_hours")] public int WeeklyHours { get; set; }
    [JsonPropertyName("is_stage_progression")] public bool IsStageProgression { get; set; }


    // Mode 1 (roadmap_overview): all nullable fields sent as null
    // Mode 2 (stage_resources): current_stage is populated; rest are null
    [JsonPropertyName("current_stage")] public CurrentStagePayload? CurrentStage { get; set; }
    [JsonPropertyName("curriculum")] public object? Curriculum { get; set; }
    [JsonPropertyName("current_stage_index")] public int? CurrentStageIndex { get; set; }
    [JsonPropertyName("learner_progress")] public object? LearnerProgress { get; set; }

    [JsonPropertyName("difficulty_level")] public string? DifficultyLevel { get; set; }
    [JsonPropertyName("skill_gaps")] public List<string>? SkillGaps { get; set; }
}

// Used only for Stage Progression (Mode 2) — sent in the same endpoint as RoadmapAIRequest but inside the optional "current_stage" field
public class CurrentStagePayload
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("topics")] public List<string> Topics { get; set; } = new();
    [JsonPropertyName("learning_objectives")] public List<string> LearningObjectives { get; set; } = new();
    [JsonPropertyName("estimated_weeks")] public int EstimatedWeeks { get; set; }
}