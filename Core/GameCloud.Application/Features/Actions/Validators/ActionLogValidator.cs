using System.Text.Json;
using FluentValidation;
using GameCloud.Application.Features.Actions.Requests;

namespace GameCloud.Application.Features.Actions.Validators;

public class ActionValidator : AbstractValidator<ActionRequest>
{
    public ActionValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");

        RuleFor(x => x.ActionType)
            .NotEmpty().WithMessage("Action type is required.")
            .MaximumLength(50).WithMessage("Action type cannot exceed 50 characters.");

        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Parameters are required.")
            .Must(BeValidJson).WithMessage("Parameters must be valid JSON.");
    }

    private bool BeValidJson(JsonElement parameters)
    {
        try
        {
        
            return true;
        }
        catch
        {
            return false;
        }
    }
}