using FastEndpoints;

public sealed class HidePostEndpoint(IPostStore posts) : Endpoint<PostByIdRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Put("/api/posts/{id}/hide");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(PostByIdRequest req, CancellationToken ct)
    {
        var hidden = posts.Hide(req.Id);

        if (hidden is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(hidden), ct);
    }
}

