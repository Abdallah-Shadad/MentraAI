using FluentValidation;

namespace MentraAI.API.Modules.CareerTracks.DTOs.Requests;

public class SelectTrackRequest
{
    public int CareerTrackId { get; set; }
}

public class SelectTrackRequestValidator : AbstractValidator<SelectTrackRequest>
{
    public SelectTrackRequestValidator()
    {
        RuleFor(x => x.CareerTrackId)
            .GreaterThan(0).WithMessage("CareerTrackId must be a valid positive integer.");
    }
}