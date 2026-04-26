using FluentValidation;

namespace MentraAI.API.Modules.Onboarding.DTOs.Requests;

public class SubmitAnswersRequest
{
    public List<AnswerItem> Answers { get; set; } = new();
}

public class AnswerItem
{
    public int QuestionId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}

public class SubmitAnswersRequestValidator : AbstractValidator<SubmitAnswersRequest>
{
    public SubmitAnswersRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotNull().WithMessage("Answers list cannot be null.")
            .NotEmpty().WithMessage("Answers list cannot be empty.");

        RuleForEach(x => x.Answers).ChildRules(item =>
        {
            item.RuleFor(a => a.QuestionId)
                .GreaterThan(0).WithMessage("QuestionId must be a valid positive integer.");

            item.RuleFor(a => a.AnswerText)
                .NotNull().WithMessage("AnswerText cannot be null.")
                .NotEmpty().WithMessage("AnswerText cannot be empty.")
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("AnswerText cannot be whitespace only.");
        });
    }
}