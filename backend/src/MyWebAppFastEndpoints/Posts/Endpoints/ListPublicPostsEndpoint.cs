using System.Security.Claims;
using FastEndpoints;

public sealed class ListPublicPostsEndpoint(IPostStore posts) : EndpointWithoutRequest<List<PublicPostResponse>>
{
    public override void Configure()
    {
        Get("/api/public/posts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Guid? viewerId = null;
        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idRaw, out var id))
            viewerId = id;

        var response = posts.GetPublic(viewerId).Select(PublicPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}

