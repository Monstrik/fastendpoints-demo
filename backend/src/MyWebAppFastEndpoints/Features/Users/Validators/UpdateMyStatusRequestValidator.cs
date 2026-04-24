namespace MyWebAppFastEndpoints.Users.Validators;

using FastEndpoints;
using FluentValidation;

/// <summary>
/// Validates UpdateMyStatusRequest for allowed status values.
/// </summary>
public sealed class UpdateMyStatusRequestValidator : Validator<UpdateMyStatusRequest>
{
    public UpdateMyStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.");

        RuleFor(x => x.Status)
            .Must(s => UserStatuses.Allowed.Contains(s))
            .WithMessage("Status is not allowed.");
    }
}
