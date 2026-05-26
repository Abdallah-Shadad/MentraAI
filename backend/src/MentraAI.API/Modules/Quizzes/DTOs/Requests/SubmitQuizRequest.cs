using FluentValidation;

namespace MentraAI.API.Modules.Quizzes.DTOs.Requests;

public class GenerateQuizRequest
{
    public Guid StageProgressId { get; set; }
}

public class GenerateQuizRequestValidator : AbstractValidator<GenerateQuizRequest>
{
    public GenerateQuizRequestValidator()
    {
        RuleFor(x => x.StageProgressId)
            .NotEmpty().WithMessage("StageProgressId is required.");
    }
}

public class SubmitQuizRequest
{
    public List<QuizAnswerItem> Answers { get; set; } = new();
}

public class QuizAnswerItem
{
    public string QuestionId { get; set; } = string.Empty;
    // Now carries the LABEL ("A", "B", "C", "D") not the full text
    public string Answer { get; set; } = string.Empty;
}

public class SubmitQuizRequestValidator : AbstractValidator<SubmitQuizRequest>
{
    public SubmitQuizRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotNull().WithMessage("Answers list cannot be null.")
            .NotEmpty().WithMessage("Answers list cannot be empty.");

        RuleForEach(x => x.Answers).ChildRules(item =>
        {
            item.RuleFor(a => a.QuestionId)
                .NotEmpty().WithMessage("QuestionId cannot be empty.");

            item.RuleFor(a => a.Answer)
                .NotEmpty().WithMessage("Answer cannot be empty.");
        });
    }
}