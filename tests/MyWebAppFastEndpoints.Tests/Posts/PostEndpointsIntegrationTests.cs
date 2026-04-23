using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public class PostEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PostEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthenticatedUser_CanCreatePost_AndAnonymousCanReadIt()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"post-user-{Guid.NewGuid():N}";
        var createdUser = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login,
            Password = "User123!",
            FirstName = "Post",
            LastName = "User",
            Role = UserRole.User
        });

        Assert.Equal(HttpStatusCode.Created, createdUser.StatusCode);

        var userToken = await LoginAndGetToken(login, "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var uniqueContent = $"hello from {login}";
        var createPost = await _client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Content = uniqueContent });
        Assert.Equal(HttpStatusCode.Created, createPost.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var listResponse = await _client.GetAsync("/api/public/posts");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var posts = await listResponse.Content.ReadFromJsonAsync<List<PublicPostResponse>>();
        Assert.NotNull(posts);
        Assert.Contains(posts!, p => p.Content == uniqueContent && p.AuthorLogin == login);
    }

    [Fact]
    public async Task Admin_CanHidePost_AndHiddenPostIsNotPublic()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var content = $"to-be-hidden-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Content = content });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PublicPostResponse>();
        Assert.NotNull(created);

        var hideResponse = await _client.PutAsJsonAsync($"/api/posts/{created!.Id}/hide", new { });
        Assert.Equal(HttpStatusCode.OK, hideResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var listResponse = await _client.GetAsync("/api/public/posts");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var posts = await listResponse.Content.ReadFromJsonAsync<List<PublicPostResponse>>();
        Assert.NotNull(posts);
        Assert.DoesNotContain(posts!, p => p.Id == created.Id);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var unhideResponse = await _client.PutAsJsonAsync($"/api/posts/{created.Id}/unhide", new { });
        Assert.Equal(HttpStatusCode.OK, unhideResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var listAfterUnhideResponse = await _client.GetAsync("/api/public/posts");
        Assert.Equal(HttpStatusCode.OK, listAfterUnhideResponse.StatusCode);

        var postsAfterUnhide = await listAfterUnhideResponse.Content.ReadFromJsonAsync<List<PublicPostResponse>>();
        Assert.NotNull(postsAfterUnhide);
        Assert.Contains(postsAfterUnhide!, p => p.Id == created.Id);
    }

    [Fact]
    public async Task RegularUser_CannotHidePost()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var content = $"admin-post-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Content = content });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PublicPostResponse>();
        Assert.NotNull(created);

        var login = $"hide-user-{Guid.NewGuid():N}";
        _ = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login,
            Password = "User123!",
            FirstName = "Hide",
            LastName = "User",
            Role = UserRole.User
        });

        var userToken = await LoginAndGetToken(login, "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var hideResponse = await _client.PutAsJsonAsync($"/api/posts/{created!.Id}/hide", new { });
        Assert.Equal(HttpStatusCode.Forbidden, hideResponse.StatusCode);

        var adminListResponse = await _client.GetAsync("/api/admin/posts");
        Assert.Equal(HttpStatusCode.Forbidden, adminListResponse.StatusCode);
    }

    [Fact]
    public async Task MyPosts_ReturnsOwnPostsIncludingHidden_Only()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var ownerLogin = $"owner-{Guid.NewGuid():N}";
        var otherLogin = $"other-{Guid.NewGuid():N}";

        _ = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = ownerLogin,
            Password = "User123!",
            FirstName = "Owner",
            LastName = "User",
            Role = UserRole.User
        });

        _ = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = otherLogin,
            Password = "User123!",
            FirstName = "Other",
            LastName = "User",
            Role = UserRole.User
        });

        var ownerToken = await LoginAndGetToken(ownerLogin, "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var visibleContent = $"visible-{Guid.NewGuid():N}";
        var hiddenContent = $"hidden-{Guid.NewGuid():N}";

        var visibleCreate = await _client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Content = visibleContent });
        Assert.Equal(HttpStatusCode.Created, visibleCreate.StatusCode);

        var hiddenCreate = await _client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Content = hiddenContent });
        Assert.Equal(HttpStatusCode.Created, hiddenCreate.StatusCode);

        var hiddenPost = await hiddenCreate.Content.ReadFromJsonAsync<PublicPostResponse>();
        Assert.NotNull(hiddenPost);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var hideResponse = await _client.PutAsJsonAsync($"/api/posts/{hiddenPost!.Id}/hide", new { });
        Assert.Equal(HttpStatusCode.OK, hideResponse.StatusCode);

        var otherToken = await LoginAndGetToken(otherLogin, "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        _ = await _client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Content = $"other-{Guid.NewGuid():N}" });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var myPostsResponse = await _client.GetAsync("/api/me/posts");
        Assert.Equal(HttpStatusCode.OK, myPostsResponse.StatusCode);

        var myPosts = await myPostsResponse.Content.ReadFromJsonAsync<List<MyPostResponse>>();
        Assert.NotNull(myPosts);
        Assert.Contains(myPosts!, p => p.Content == visibleContent && !p.IsHidden);
        Assert.Contains(myPosts!, p => p.Content == hiddenContent && p.IsHidden);
        Assert.DoesNotContain(myPosts!, p => p.AuthorLogin == otherLogin);
    }

    private async Task<string> LoginAndGetToken(string login, string password)
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Login = login,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);

        return body!.Token;
    }
}

