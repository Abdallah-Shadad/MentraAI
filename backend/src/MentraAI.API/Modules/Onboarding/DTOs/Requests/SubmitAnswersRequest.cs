using FluentValidation;
using System.Linq;
using System.Collections.Generic;

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
    // Required question IDs based on the onboarding flow: Age (1), Education (2), Employment (5), Current Skills (10), Future Skills (11)
    private readonly HashSet<int> _requiredQuestionIds = new() { 1, 2, 5, 10, 11 };

    public SubmitAnswersRequestValidator()
    {
        // Validate that the Answers list is not null or empty
        RuleFor(x => x.Answers)
            .NotNull().WithMessage("Answers list cannot be null.")
            .NotEmpty().WithMessage("Answers list cannot be empty.");
        // Validate that all required questions are answered
        RuleFor(x => x.Answers)
            .Must(answers => answers != null && _requiredQuestionIds.All(id => answers.Any(a => a.QuestionId == id)))
            .WithMessage("You must answer all required fields: Age, Education, Employment, Current Skills, and Future Skills.");
        // Validate each answer item
        RuleForEach(x => x.Answers).ChildRules(item =>
        {
            item.RuleFor(a => a.QuestionId)
                .GreaterThan(0).WithMessage("QuestionId must be a valid positive integer.");

            item.RuleFor(a => a.AnswerText)
                .NotNull().WithMessage("Answer text cannot be null for required fields.")
                .NotEmpty().WithMessage("Answer text cannot be empty for required fields.")
                .Must(v => !string.IsNullOrWhiteSpace(v) && v.Trim() != "[]")
                .WithMessage("Answer text cannot be empty or an empty array [] for required fields.")
                .When(a => _requiredQuestionIds.Contains(a.QuestionId));
        });
    }
}