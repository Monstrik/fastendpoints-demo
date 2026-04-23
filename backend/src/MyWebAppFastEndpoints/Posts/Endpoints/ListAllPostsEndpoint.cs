using System.Security.Claims;
using FastEndpoints;

public sealed class ListAllPostsEndpoint(IPostStore posts) : EndpointWithoutRequest<List<MyPostResponse>>
{
    public override void Configure()
    {
        Get("/api/admin/posts");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Guid? viewerId = null;
        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idRaw, out var id))
            viewerId = id;

        var response = posts.GetAll(viewerId).Select(MyPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}

