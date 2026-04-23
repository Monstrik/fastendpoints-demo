using System.Security.Claims;
using FastEndpoints;

public sealed class CreatePostRequest
{
    public required string Content { get; set; }
}

public sealed class PostByIdRequest
{
    public Guid Id { get; set; }
}

public sealed class PublicPostResponse
{
    public Guid Id { get; set; }
    public required string AuthorLogin { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public static PublicPostResponse From(AppPost post) => new()
    {
        Id = post.Id,
        AuthorLogin = post.AuthorLogin,
        Content = post.Content,
        CreatedAtUtc = post.CreatedAtUtc
    };
}

public sealed class MyPostResponse
{
    public Guid Id { get; set; }
    public required string AuthorLogin { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsHidden { get; set; }

    public static MyPostResponse From(AppPost post) => new()
    {
        Id = post.Id,
        AuthorLogin = post.AuthorLogin,
        Content = post.Content,
        CreatedAtUtc = post.CreatedAtUtc,
        IsHidden = post.IsHidden
    };
}

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

public sealed class ListPublicPostsEndpoint(IPostStore posts) : EndpointWithoutRequest<List<PublicPostResponse>>
{
    public override void Configure()
    {
        Get("/api/public/posts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = posts.GetPublic().Select(PublicPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}

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

        var response = posts.GetByAuthor(user.Id).Select(MyPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}

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

