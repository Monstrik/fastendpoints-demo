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

public sealed class PostReactionRequest
{
    public Guid Id { get; set; }
    public required string Reaction { get; set; }
}

public sealed class PublicPostResponse
{
    public Guid Id { get; set; }
    public required string AuthorLogin { get; set; }
    public required string AuthorStatus { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public string? ViewerReaction { get; set; }

    public static PublicPostResponse From(AppPost post) => new()
    {
        Id = post.Id,
        AuthorLogin = post.AuthorLogin,
        AuthorStatus = post.AuthorStatus,
        Content = post.Content,
        CreatedAtUtc = post.CreatedAtUtc,
        LikesCount = post.LikesCount,
        DislikesCount = post.DislikesCount,
        ViewerReaction = post.ViewerReaction?.ToString()
    };
}

public sealed class MyPostResponse
{
    public Guid Id { get; set; }
    public required string AuthorLogin { get; set; }
    public required string AuthorStatus { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsHidden { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public string? ViewerReaction { get; set; }

    public static MyPostResponse From(AppPost post) => new()
    {
        Id = post.Id,
        AuthorLogin = post.AuthorLogin,
        AuthorStatus = post.AuthorStatus,
        Content = post.Content,
        CreatedAtUtc = post.CreatedAtUtc,
        IsHidden = post.IsHidden,
        LikesCount = post.LikesCount,
        DislikesCount = post.DislikesCount,
        ViewerReaction = post.ViewerReaction?.ToString()
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
        Guid? viewerId = null;
        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idRaw, out var id))
            viewerId = id;

        var response = posts.GetPublic(viewerId).Select(PublicPostResponse.From).ToList();
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

        var response = posts.GetByAuthor(user.Id, user.Id).Select(MyPostResponse.From).ToList();
        await Send.OkAsync(response, ct);
    }
}

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

public sealed class SetPostReactionEndpoint(IPostStore posts) : Endpoint<PostReactionRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Put("/api/posts/{id}/reaction");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(PostReactionRequest req, CancellationToken ct)
    {
        var normalized = req.Reaction.Trim().ToLowerInvariant();
        var reaction = normalized switch
        {
            "like" => PostReactionType.Like,
            "dislike" => PostReactionType.Dislike,
            _ => (PostReactionType?)null
        };

        if (reaction is null)
        {
            AddError(r => r.Reaction, "Reaction must be either 'like' or 'dislike'.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idRaw, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var updated = posts.SetReaction(req.Id, userId, reaction.Value);
        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
    }
}

public sealed class ClearPostReactionEndpoint(IPostStore posts) : Endpoint<PostByIdRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Delete("/api/posts/{id}/reaction");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(PostByIdRequest req, CancellationToken ct)
    {
        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idRaw, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var updated = posts.ClearReaction(req.Id, userId);
        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
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

public sealed class UnhidePostEndpoint(IPostStore posts) : Endpoint<PostByIdRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Put("/api/posts/{id}/unhide");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(PostByIdRequest req, CancellationToken ct)
    {
        var updated = posts.Unhide(req.Id);
        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
    }
}

