namespace MyWebAppFastEndpoints.Posts.Validators;

using FastEndpoints;
using FluentValidation;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Validates PostReactionRequest for valid reaction types.
/// </summary>
public sealed class PostReactionRequestValidator : Validator<PostReactionRequest>
{
    public PostReactionRequestValidator()
    {
        RuleFor(x => x.Reaction)
            .NotEmpty()
            .WithMessage("Reaction is required.");

        RuleFor(x => x.Reaction)
            .Must(r => new[] { AppConstants.Reactions.Like, AppConstants.Reactions.Dislike }.Contains(r.Trim().ToLowerInvariant()))
            .WithMessage($"Reaction must be either '{AppConstants.Reactions.Like}' or '{AppConstants.Reactions.Dislike}'.");

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Post ID is required.");
    }
}
