using FastEndpoints;

public sealed class UnhidePostEndpoint(IPostStore posts) : Endpoint<PostByIdRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Put("/api/posts/{id}/unhide");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(PostByIdRequest req, CancellationToken ct)
    {
        AppPost? updated;
        try
        {
            updated = posts.Unhide(req.Id);
        }
        catch (PostNotFoundException)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
    }
}

