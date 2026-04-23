using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Sets a reaction (like or dislike) on a post by the authenticated user.
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

        if (reaction is null)
        {
            AddError(r => r.Reaction, $"Reaction must be either '{AppConstants.Reactions.Like}' or '{AppConstants.Reactions.Dislike}'.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var userId = User.GetUserId();
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var updated = posts.SetReaction(req.Id, userId.Value, reaction.Value);
        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
    }
}
