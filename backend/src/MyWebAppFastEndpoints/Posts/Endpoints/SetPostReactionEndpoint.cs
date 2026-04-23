using System.Security.Claims;
using FastEndpoints;

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
            "like" => PostReactionType.Like,
            "dislike" => PostReactionType.Dislike,
            _ => (PostReactionType?)null
        };

        if (reaction is null)
        {
            AddError(r => r.Reaction, "Reaction must be either 'like' or 'dislike'.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idRaw, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var updated = posts.SetReaction(req.Id, userId, reaction.Value);
        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
    }
}

