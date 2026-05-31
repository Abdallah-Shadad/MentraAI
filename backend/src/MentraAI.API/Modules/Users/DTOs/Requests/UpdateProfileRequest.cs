using FluentValidation;

namespace MentraAI.API.Modules.Users.DTOs.Requests;

public class UpdateProfileRequest
{
    public string? Age { get; set; }
    public string? EdLevel { get; set; }
    public double? YearsCode { get; set; }
    public double? WorkExp { get; set; }
    public string? Employment { get; set; }
    public string? RemoteWork { get; set; }
    public string? Industry { get; set; }
    public string? OrgSize { get; set; }
    public string? AISelect { get; set; }
    public List<string>? CurrentSkills { get; set; }
    public List<string>? FutureSkills { get; set; }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    private static readonly HashSet<string> ValidAges = new(StringComparer.Ordinal) { "18-24 years old", "25-34 years old", "35-44 years old", "45-54 years old", "55-64 years old", "65 years or older", "Prefer not to say" };
    private static readonly HashSet<string> ValidEdLevels = new(StringComparer.Ordinal) { "Primary/elementary school", "Secondary school (e.g. American high school, German Realschule or Gymnasium, etc.)", "Some college/university study without earning a degree", "Associate degree (A.A., A.S., etc.)", "Bachelor's degree (B.A., B.S., B.Eng., etc.)", "Master's degree (M.A., M.S., M.Eng., MBA, etc.)", "Professional degree (JD, MD, Ph.D, Ed.D, etc.)", "Other (please specify):" };
    private static readonly HashSet<string> ValidEmployments = new(StringComparer.Ordinal) { "Employed", "Independent contractor, freelancer, or self-employed", "Student", "Not employed", "I prefer not to say" };
    private static readonly HashSet<string> ValidRemoteWork = new(StringComparer.Ordinal) { "Remote", "Hybrid (some in-person, leans heavy to flexibility)", "Hybrid (some remote, leans heavy to in-person)", "Your choice (very flexible, you can come in when you want or just as needed)", "In-person" };
    private static readonly HashSet<string> ValidIndustries = new(StringComparer.Ordinal) { "Software Development", "Computer Systems Design and Services", "Internet, Telecomm or Information Services", "Fintech", "Banking/Financial Services", "Insurance", "Healthcare", "Retail and Consumer Services", "Manufacturing", "Transportation, or Supply Chain", "Energy", "Government", "Higher Education", "Media & Advertising Services", "Other:", "null" };
    private static readonly HashSet<string> ValidOrgSizes = new(StringComparer.Ordinal) { "Just me - I am a freelancer, sole proprietor, etc.", "Less than 20 employees", "20 to 99 employees", "100 to 499 employees", "500 to 999 employees", "1,000 to 4,999 employees", "5,000 to 9,999 employees", "10,000 or more employees", "I don't know", "null" };
    private static readonly HashSet<string> ValidAISelect = new(StringComparer.Ordinal) { "Yes, I use AI tools daily", "Yes, I use AI tools weekly", "Yes, I use AI tools monthly or infrequently", "No, but I plan to soon", "No, and I don't plan to", "null" };

    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Age).Must(v => ValidAges.Contains(v!)).WithMessage("Invalid Age.").When(x => x.Age is not null);
        RuleFor(x => x.EdLevel).Must(v => ValidEdLevels.Contains(v!)).WithMessage("Invalid EdLevel.").When(x => x.EdLevel is not null);
        RuleFor(x => x.Employment).Must(v => ValidEmployments.Contains(v!)).WithMessage("Invalid Employment.").When(x => x.Employment is not null);
        RuleFor(x => x.RemoteWork).Must(v => ValidRemoteWork.Contains(v!)).WithMessage("Invalid RemoteWork.").When(x => x.RemoteWork is not null);
        RuleFor(x => x.Industry).Must(v => ValidIndustries.Contains(v!)).WithMessage("Invalid Industry.").When(x => x.Industry is not null);
        RuleFor(x => x.OrgSize).Must(v => ValidOrgSizes.Contains(v!)).WithMessage("Invalid OrgSize.").When(x => x.OrgSize is not null);
        RuleFor(x => x.AISelect).Must(v => ValidAISelect.Contains(v!)).WithMessage("Invalid AISelect.").When(x => x.AISelect is not null);

        RuleFor(x => x.YearsCode).GreaterThanOrEqualTo(0).LessThanOrEqualTo(60).When(x => x.YearsCode.HasValue);
        RuleFor(x => x.WorkExp).GreaterThanOrEqualTo(0).LessThanOrEqualTo(60).When(x => x.WorkExp.HasValue);

        RuleFor(x => x.CurrentSkills).Must(s => s!.Count > 0).WithMessage("Cannot be empty.").When(x => x.CurrentSkills is not null);
        RuleFor(x => x.FutureSkills).Must(s => s!.Count > 0).WithMessage("Cannot be empty.").When(x => x.FutureSkills is not null);
    }
}