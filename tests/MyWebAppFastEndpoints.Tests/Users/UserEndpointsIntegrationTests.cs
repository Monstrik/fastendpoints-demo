using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public class UserEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_CreateThenGetById_WorksEndToEnd()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createRequest = new CreateUserRequest
        {
            Login = "aya-user",
            Password = "User123!",
            FirstName = "Aya",
            LastName = "Kovi",
            Age = 21,
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
            Age = 20,
            Role = UserRole.User
        });

        Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);
    }

    [Fact]
    public async Task RegularUser_CannotListUsers_ButCanReadOwnProfile()
    {
        var adminToken = await LoginAndGetToken("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        _ = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Login = "regular-user",
            Password = "User123!",
            FirstName = "Regular",
            LastName = "User",
            Age = 22,
            Role = UserRole.User
        });

        var userToken = await LoginAndGetToken("regular-user", "User123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var listResponse = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);

        var meResponse = await _client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(me);
        Assert.Equal("regular-user", me!.Login);
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
