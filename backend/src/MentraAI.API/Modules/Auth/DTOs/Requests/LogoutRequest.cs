using FluentValidation;

namespace MentraAI.API.Modules.Auth.DTOs.Requests;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}