using FluentValidation;
using GameCloud.Application.Features.Games.Requests;

namespace GameCloud.Application.Features.Games;

public class GameValidator : AbstractValidator<GameRequest>
{
    public GameValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3, 100).WithMessage("Name must be between 3 and 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.DeveloperId)
            .NotEmpty().WithMessage("Developer ID is required.");
    }
}