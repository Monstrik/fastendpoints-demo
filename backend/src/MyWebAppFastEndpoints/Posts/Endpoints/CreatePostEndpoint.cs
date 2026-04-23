using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Creates a new post with validation for content length and author authentication.
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
        var content = req.Content?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(content))
        {
            AddError(r => r.Content, "Post content is required.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (content.Length > AppConstants.PostMaxContentLength)
        {
            AddError(r => r.Content, $"Post content must be {AppConstants.PostMaxContentLength} characters or less.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

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
