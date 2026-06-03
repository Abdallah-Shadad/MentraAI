using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;
using MentraAI.API.Modules.Quizzes.Models;

namespace MentraAI.API.Modules.Quizzes.Mappings;

public class QuizMappingProfile : Profile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QuizMappingProfile()
    {
        CreateMap<QuizAttempt, QuizResponse>()
            .ForMember(d => d.QuizId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.PassingScore, o => o.MapFrom(s => s.PassingScore))      // NEW
            .ForMember(d => d.TimeLimitMinutes, o => o.MapFrom(s => s.TimeLimitMinutes))  // NEW
            .ForMember(d => d.Questions, o => o.MapFrom(s => ResolveQuestions(s.QuestionsDataJson)));

        CreateMap<QuizAttempt, QuizAttemptSummary>()
            .ForMember(d => d.QuizId, o => o.MapFrom(s => s.Id));
    }

    public static List<QuizQuestionItem> ResolveQuestions(string questionsDataJson)
    {
        if (string.IsNullOrWhiteSpace(questionsDataJson))
            return new List<QuizQuestionItem>();

        try
        {
            var stored = JsonSerializer
                .Deserialize<List<StoredQuestionRaw>>(questionsDataJson, JsonOptions)
                ?? new List<StoredQuestionRaw>();

            return stored
                .Select(q => new QuizQuestionItem
                {
                    Id = q.QuestionId,
                    Text = q.QuestionText,
                    Choices = q.Choices?
                        .Select(c => new ChoiceItem
                        {
                            Label = c.Label,
                            Text = c.Text
                        })
                        .ToList() ?? new List<ChoiceItem>()
                })
                .ToList();
        }
        catch
        {
            return new List<QuizQuestionItem>();
        }
    }

    // Mirrors the new AI question structure stored in QuestionsDataJson
    public class StoredQuestionRaw
    {
        [JsonPropertyName("question_id")] public string QuestionId { get; set; } = string.Empty;
        [JsonPropertyName("question_text")] public string QuestionText { get; set; } = string.Empty;
        [JsonPropertyName("choices")] public List<StoredChoice> Choices { get; set; } = new();
    }

    public class StoredChoice
    {
        [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
    }
}