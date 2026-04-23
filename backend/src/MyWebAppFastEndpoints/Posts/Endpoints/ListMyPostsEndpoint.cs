using System.Security.Claims;
using FastEndpoints;

public sealed class ListMyPostsEndpoint(IUserStore users, IPostStore posts) : EndpointWithoutRequest<List<MyPostResponse>>
{
    public override void Configure()
    {
        Get("/api/me/posts");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idRaw, out var id))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = users.GetById(id);
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var response = posts.GetByAuthor(user.Id, user.Id).Select(MyPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}

