using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Creates a new post with validation for content length and author authentication.
/// Validation is handled by FastEndpoints validator (CreatePostRequestValidator).
/// </summary>
public sealed class CreatePostEndpoint(IUserStore users, IPostStore posts) : Endpoint<CreatePostRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Post("/api/posts");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(CreatePostRequest req, CancellationToken ct)
    {
        var content = req.Content.Trim();

        var userId = User.GetUserId();
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var author = users.GetById(userId.Value);
        if (author is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var created = posts.Create(author.Id, author.Login, content);
        await Send.CreatedAtAsync<ListPublicPostsEndpoint>(new { }, PublicPostResponse.From(created), cancellation: ct);
    }
}
