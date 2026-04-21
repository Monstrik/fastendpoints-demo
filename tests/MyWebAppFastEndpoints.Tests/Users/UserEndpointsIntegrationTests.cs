using System.Net;
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
    public async Task CreateThenGetById_WorksEndToEnd()
    {
        var createRequest = new CreateUserRequest
        {
            FirstName = "Aya",
            LastName = "Kovi",
            Age = 21
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
        Assert.True(loaded.IsOver18);
    }
}

