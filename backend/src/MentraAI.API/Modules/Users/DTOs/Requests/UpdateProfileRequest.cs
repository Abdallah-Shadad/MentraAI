using FluentValidation;

namespace MentraAI.API.Modules.Users.DTOs.Requests;

public class UpdateProfileRequest
{
    public string? Background { get; set; }
    public List<string>? CurrentSkills { get; set; }
    public List<string>? Interests { get; set; }
    public int? WeeklyHours { get; set; }
    public string? CareerGoals { get; set; }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Background)
            .MaximumLength(500).WithMessage("Background cannot exceed 500 characters.")
            .When(x => x.Background is not null);

        RuleFor(x => x.CareerGoals)
            .MaximumLength(500).WithMessage("Career goals cannot exceed 500 characters.")
            .When(x => x.CareerGoals is not null);

        RuleFor(x => x.WeeklyHours)
            .InclusiveBetween(1, 168).WithMessage("Weekly hours must be between 1 and 168.")
            .When(x => x.WeeklyHours is not null);

        RuleFor(x => x.CurrentSkills)
            .Must(s => s!.Count > 0).WithMessage("Skills list cannot be empty.")
            .When(x => x.CurrentSkills is not null);

        RuleFor(x => x.Interests)
            .Must(i => i!.Count > 0).WithMessage("Interests list cannot be empty.")
            .When(x => x.Interests is not null);
    }
}