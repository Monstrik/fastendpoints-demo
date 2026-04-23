namespace MyWebAppFastEndpoints.Posts.Validators;

using FastEndpoints;
using FluentValidation;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Validates CreatePostRequest for content length and required fields.
/// </summary>
public sealed class CreatePostRequestValidator : Validator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Post content is required.");

        RuleFor(x => x.Content)
            .MaximumLength(AppConstants.PostMaxContentLength)
            .WithMessage($"Post content must be {AppConstants.PostMaxContentLength} characters or less.");
    }
}
