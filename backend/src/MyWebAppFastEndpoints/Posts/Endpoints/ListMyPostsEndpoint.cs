using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Retrieves the authenticated user's own posts, including hidden ones.
/// </summary>
public sealed class ListMyPostsEndpoint(IUserStore users, IPostStore posts) : EndpointWithoutRequest<List<MyPostResponse>>
{
    public override void Configure()
    {
        Get("/api/me/posts");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = users.GetById(userId.Value);
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var response = posts.GetByAuthor(user.Id, user.Id).Select(MyPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}
