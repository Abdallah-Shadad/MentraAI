using FluentValidation;

namespace MentraAI.API.Modules.Chat.DTOs.Requests;

// What the FRONTEND sends — userId is injected from JWT, never from client.
// ConversationId comes from the URL route parameter, not the body.
public class ChatRequest
{
    public string Query { get; set; } = string.Empty;
    public string? CareerTrack { get; set; }
    public string? Stage { get; set; }
    public string? LessonId { get; set; }
    public string? QuizDetails { get; set; }
    public int? QuizScore { get; set; }
}

public class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query cannot be empty.")
            .MaximumLength(2000).WithMessage("Query cannot exceed 2000 characters.");

        RuleFor(x => x.QuizScore)
            .InclusiveBetween(0, 100).WithMessage("QuizScore must be between 0 and 100.")
            .When(x => x.QuizScore.HasValue);
    }
}