using FluentValidation;

namespace MentraAI.API.Modules.Chat.DTOs.Requests;

/// <summary>
/// What the FRONTEND sends to OUR backend.
/// user_id is intentionally absent — it is injected from the JWT on the backend.
/// </summary>
public class ChatRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;

    // Optional context — frontend fills from its current state
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
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query cannot be empty.")
            .MaximumLength(2000).WithMessage("Query cannot exceed 2000 characters.");
    }
}