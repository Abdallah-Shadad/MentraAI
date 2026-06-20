using FluentValidation;

namespace MentraAI.API.Modules.Auth.DTOs.Requests;

public class GitHubLoginRequest
{
    public string Code { get; set; } = string.Empty;
}

public class GitHubLoginRequestValidator : AbstractValidator<GitHubLoginRequest>
{
    public GitHubLoginRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Authorization code is required.");
    }
}
