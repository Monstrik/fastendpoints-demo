using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

public sealed class ListAllPostsEndpoint(IPostStore posts) : EndpointWithoutRequest<List<MyPostResponse>>
{
    public override void Configure()
    {
        Get("/api/admin/posts");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var viewerId = User.GetUserId();

        var response = posts.GetAll(viewerId).Select(MyPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}
