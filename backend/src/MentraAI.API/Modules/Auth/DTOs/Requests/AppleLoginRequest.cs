using FluentValidation;

namespace MentraAI.API.Modules.Auth.DTOs.Requests;

public class AppleLoginRequest
{
    public string IdentityToken { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class AppleLoginRequestValidator : AbstractValidator<AppleLoginRequest>
{
    public AppleLoginRequestValidator()
    {
        RuleFor(x => x.IdentityToken)
            .NotEmpty().WithMessage("IdentityToken is required.");
    }
}
