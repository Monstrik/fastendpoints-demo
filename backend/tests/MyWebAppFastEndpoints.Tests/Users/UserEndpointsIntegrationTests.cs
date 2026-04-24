using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class UserEndpointsIntegrationTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserEndpointsIntegrationTests(TestsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_CreateThenGetById_WorksEndToEnd()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"aya-user-{Guid.NewGuid():N}";

        var createRequest = new CreateUserRequest
        {
            Login = login,
            Password = "User123!",
            FirstName = "Aya",
            LastName = "Kovi",
            Role = UserRole.User
        };

        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/users/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var loaded = await getResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded!.Id);
        Assert.Equal("Aya Kovi", loaded.FullName);
        Assert.Equal("User", loaded.Role);
    }

    [Fact]
    public async Task Anonymous_CannotManageUsers()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = "anonymous-create",
            Password = "User123!",
            FirstName = "Anon",
            LastName = "User",
            Role = UserRole.User
        });

        Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);
    }

    [Fact]
    public async Task RegularUser_CannotListUsers_ButCanReadOwnProfile()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"regular-user-{Guid.NewGuid():N}";

        _ = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login,
            Password = "User123!",
            FirstName = "Regular",
            LastName = "User",
            Role = UserRole.User
        });

        var userToken = await LoginAndGetToken(login, "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var listResponse = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);

        var meResponse = await _client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(me);
        Assert.Equal(login, me!.Login);
    }

    [Fact]
    public async Task RegularUser_CanUpdateOwnStatus()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"status-user-{Guid.NewGuid():N}";

        _ = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login,
            Password = "User123!",
            FirstName = "Status",
            LastName = "User",
            Role = UserRole.User
        });

        var userToken = await LoginAndGetToken(login, "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var updateResponse = await _client.PutAsJsonAsync("/api/me/status", new { status = "🎯 Focused" });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var meResponse = await _client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(me);
        Assert.Equal("🎯 Focused", me!.Status);
    }

    [Fact]
    public async Task Anonymous_CanReadPublicUserStatuses()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/public/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<PublicUserStatusResponse>>();
        Assert.NotNull(users);
        Assert.Contains(users!, u => !string.IsNullOrWhiteSpace(u.Login));
        Assert.Contains(users!, u => !string.IsNullOrWhiteSpace(u.Status));
    }

    [Fact]
    public async Task Admin_CanListUsers()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        Assert.NotNull(users);
        Assert.Contains(users!, u => u.Login == "admin");
    }

    [Fact]
    public async Task Admin_CanUpdateUser()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"update-me-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login,
            Password = "User123!",
            FirstName = "Before",
            LastName = "Update",
            Role = UserRole.User
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{created!.Id}", new UpdateUserRequest
        {
            Id = created.Id,
            Login = login,
            FirstName = "After",
            LastName = "Update",
            Role = UserRole.User
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(updated);
        Assert.Equal("After", updated!.FirstName);
        Assert.Equal("After Update", updated.FullName);
    }

    [Fact]
    public async Task Admin_CanDeleteUser()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"delete-me-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login,
            Password = "User123!",
            FirstName = "To",
            LastName = "Delete",
            Role = UserRole.User
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_DeleteNonExistentUser_ReturnsNotFound()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var deleteResponse = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_CreateUser_WhenLoginExists_ReturnsError()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"duplicate-{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login, Password = "User123!", FirstName = "First", LastName = "User", Role = UserRole.User
        });

        var duplicate = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login, Password = "User123!", FirstName = "Second", LastName = "User", Role = UserRole.User
        });

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
    }

    [Fact]
    public async Task Anonymous_ForgotPassword_AlwaysReturnsOk()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var existingResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { login = "admin" });
        Assert.Equal(HttpStatusCode.OK, existingResponse.StatusCode);

        var nonExistingResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { login = "doesnotexist" });
        Assert.Equal(HttpStatusCode.OK, nonExistingResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateMyStatus_InvalidStatus_ReturnsBadRequest()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var login = $"bad-status-{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = login, Password = "User123!", FirstName = "Bad", LastName = "Status", Role = UserRole.User
        });

        var userToken = await LoginAndGetToken(login, "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var response = await _client.PutAsJsonAsync("/api/me/status", new { status = "not a valid status" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
