using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Responses;

public class RoadmapAIResponse
{
    [JsonPropertyName("signal")] public string Signal { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("roadmap")] public RoadmapPayload? Roadmap { get; set; }
}

public class RoadmapPayload
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("mode")] public string Mode { get; set; } = string.Empty;
    [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("career_track")] public string CareerTrack { get; set; } = string.Empty;
    [JsonPropertyName("data")] public RoadmapData? Data { get; set; }
}

public class RoadmapData
{
    [JsonPropertyName("difficulty_level")] public string DifficultyLevel { get; set; } = string.Empty;
    [JsonPropertyName("skill_gaps")] public List<string> SkillGaps { get; set; } = new();
    [JsonPropertyName("total_weeks")] public int TotalWeeks { get; set; }
    [JsonPropertyName("curriculum")] public RoadmapCurriculum? Curriculum { get; set; }
}

public class RoadmapCurriculum
{
    [JsonPropertyName("stages")] public List<AIStage> Stages { get; set; } = new();
}

public class AIStage
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("topics")] public List<string> Topics { get; set; } = new();
    [JsonPropertyName("estimated_weeks")] public int EstimatedWeeks { get; set; }
}