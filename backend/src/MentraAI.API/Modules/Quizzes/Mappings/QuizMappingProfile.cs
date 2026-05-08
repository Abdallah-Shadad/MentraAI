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
            .ForMember(d => d.Questions, o => o.MapFrom<QuizQuestionsResolver>());

        CreateMap<QuizAttempt, QuizAttemptSummary>()
            .ForMember(d => d.QuizId, o => o.MapFrom(s => s.Id));
    }

    private sealed class QuizQuestionsResolver : IValueResolver<QuizAttempt, QuizResponse, List<QuizQuestionItem>>
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
                var stored = JsonSerializer.Deserialize<List<StoredQuestionRaw>>(source.QuestionsDataJson, Json)
                             ?? new List<StoredQuestionRaw>();

                // CorrectAnswer intentionally ignored: StoredQuestionRaw does not model it.
                return stored
                    .Select(q => new QuizQuestionItem
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Options = q.Options
                    })
                    .ToList();
            }
            catch
            {
                return new List<QuizQuestionItem>();
            }
        }
    }

    private sealed class StoredQuestionRaw
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
        [JsonPropertyName("options")] public List<string> Options { get; set; } = new();
        // correct_answer may exist in JSON but is intentionally not modeled here.
    }
}

