using System.Security.Claims;
using FastEndpoints;

public sealed class CreatePostEndpoint(IUserStore users, IPostStore posts) : Endpoint<CreatePostRequest, PublicPostResponse>
{
    private const int MaxContentLength = 280;

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

        if (content.Length > MaxContentLength)
        {
            AddError(r => r.Content, $"Post content must be {MaxContentLength} characters or less.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idRaw, out var id))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var author = users.GetById(id);
        if (author is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var created = posts.Create(author.Id, author.Login, content);
        await Send.CreatedAtAsync<ListPublicPostsEndpoint>(new { }, PublicPostResponse.From(created), cancellation: ct);
    }
}

