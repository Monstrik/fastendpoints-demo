using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Sets a reaction (like or dislike) on a post by the authenticated user.
/// Validation is handled by FastEndpoints validator (PostReactionRequestValidator).
/// </summary>
public sealed class SetPostReactionEndpoint(IPostStore posts) : Endpoint<PostReactionRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Put("/api/posts/{id}/reaction");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(PostReactionRequest req, CancellationToken ct)
    {
        var normalized = req.Reaction.Trim().ToLowerInvariant();
        var reaction = normalized switch
        {
            AppConstants.Reactions.Like => PostReactionType.Like,
            AppConstants.Reactions.Dislike => PostReactionType.Dislike,
            _ => (PostReactionType?)null
        };

        var userId = User.GetUserId();
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var updated = posts.SetReaction(req.Id, userId.Value, reaction!.Value);
        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
    }
}
