using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Retrieves public posts, optionally filtered by the authenticated viewer's perspective.
/// </summary>
public sealed class ListPublicPostsEndpoint(IPostStore posts) : EndpointWithoutRequest<List<PublicPostResponse>>
{
    public override void Configure()
    {
        Get("/api/public/posts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var viewerId = User.GetUserId();

        var response = posts.GetPublic(viewerId).Select(PublicPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}
