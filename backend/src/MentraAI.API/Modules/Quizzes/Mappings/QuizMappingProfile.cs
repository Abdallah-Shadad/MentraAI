using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;
using MentraAI.API.Modules.Quizzes.Models;

namespace MentraAI.API.Modules.Quizzes.Mappings;

public class QuizMappingProfile : Profile
{
    public QuizMappingProfile()
    {
        CreateMap<QuizAttempt, QuizResponse>()
            .ForMember(d => d.QuizId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.PassingScore, o => o.MapFrom(s => s.PassingScore))      // NEW
            .ForMember(d => d.TimeLimitMinutes, o => o.MapFrom(s => s.TimeLimitMinutes))  // NEW
            .ForMember(d => d.Questions, o => o.MapFrom<QuizQuestionsResolver>());

        CreateMap<QuizAttempt, QuizAttemptSummary>()
            .ForMember(d => d.QuizId, o => o.MapFrom(s => s.Id));
    }

    private sealed class QuizQuestionsResolver
        : IValueResolver<QuizAttempt, QuizResponse, List<QuizQuestionItem>>
    {
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public List<QuizQuestionItem> Resolve(
            QuizAttempt source,
            QuizResponse destination,
            List<QuizQuestionItem> destMember,
            ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.QuestionsDataJson))
                return new List<QuizQuestionItem>();

            try
            {
                var stored = JsonSerializer
                    .Deserialize<List<StoredQuestionRaw>>(source.QuestionsDataJson, Json)
                    ?? new List<StoredQuestionRaw>();

                // Map new structure: question_id, question_text, choices (label + text)
                // correct_answer, explanation, hints intentionally ignored.
                return stored
                    .Select(q => new QuizQuestionItem
                    {
                        Id = q.QuestionId,
                        Text = q.QuestionText,
                        Choices = q.Choices
                            .Select(c => new ChoiceItem
                            {
                                Label = c.Label,
                                Text = c.Text
                                // is_correct intentionally absent
                            })
                            .ToList()
                    })
                    .ToList();
            }
            catch
            {
                return new List<QuizQuestionItem>();
            }
        }
    }

    // Mirrors the new AI question structure stored in QuestionsDataJson
    private sealed class StoredQuestionRaw
    {
        [JsonPropertyName("question_id")] public string QuestionId { get; set; } = string.Empty;
        [JsonPropertyName("question_text")] public string QuestionText { get; set; } = string.Empty;
        [JsonPropertyName("choices")] public List<StoredChoice> Choices { get; set; } = new();
        // correct_answer may exist in JSON but intentionally not modeled here (security)
    }

    private sealed class StoredChoice
    {
        [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
        // is_correct intentionally not modeled
    }
}