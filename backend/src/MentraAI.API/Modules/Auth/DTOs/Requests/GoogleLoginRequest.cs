using FluentValidation;

namespace MentraAI.API.Modules.Auth.DTOs.Requests;

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("IdToken is required.");
    }
}
