using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.AIGateway.DTOs.Responses;

// Response from POST /api/v1/quiz/adaptation_stage
// Top-level: signal, status, message, Additional_Resource, time_consume
public class AdaptationAIResponse
{
    [JsonPropertyName("signal")] public string Signal { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("Additional_Resource")] public AdaptationPayload? AdditionalResource { get; set; }
    [JsonPropertyName("time_consume")] public double TimeConsume { get; set; }
}

public class AdaptationPayload
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("mode")] public string Mode { get; set; } = string.Empty;
    [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("career_track")] public string CareerTrack { get; set; } = string.Empty;
    [JsonPropertyName("stage_id")] public string StageId { get; set; } = string.Empty;
    [JsonPropertyName("stage_name")] public string StageName { get; set; } = string.Empty;
    [JsonPropertyName("score")] public decimal Score { get; set; }
    [JsonPropertyName("adapted")] public bool Adapted { get; set; }
    [JsonPropertyName("data")] public AdaptationData? Data { get; set; }
    [JsonPropertyName("error")] public string? Error { get; set; }
}

public class AdaptationData
{
    [JsonPropertyName("curriculum")] public AdaptationCurriculum? Curriculum { get; set; }
    [JsonPropertyName("summary")] public string Summary { get; set; } = string.Empty;
    [JsonPropertyName("struggling_topics")] public List<string> StrugglingTopics { get; set; } = new();
    [JsonPropertyName("recommended_next_action")] public string RecommendedNextAction { get; set; } = string.Empty;
}

public class AdaptationCurriculum
{
    [JsonPropertyName("stages")] public List<AdaptationStage> Stages { get; set; } = new();
    [JsonPropertyName("last_adapted_stage")] public string? LastAdaptedStage { get; set; }
}

public class AdaptationStage
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("resources")] public List<RemediationResource> Resources { get; set; } = new();
    [JsonPropertyName("adapted")] public bool Adapted { get; set; }
    [JsonPropertyName("adaptation_summary")] public string? AdaptationSummary { get; set; }

    [JsonPropertyName("estimated_weeks")] public int EstimatedWeeks { get; set; }
}

// Each remedial resource from the AI
public class RemediationResource
{
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("duration_min")] public int DurationMin { get; set; }
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; } = string.Empty;
    [JsonPropertyName("topic")] public string Topic { get; set; } = string.Empty;
    [JsonPropertyName("why")] public string Why { get; set; } = string.Empty;
    [JsonPropertyName("resource_type")] public string ResourceType { get; set; } = string.Empty;
}