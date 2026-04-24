using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public class SwaggerEnabledIntegrationTests : IClassFixture<SwaggerEnabledWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SwaggerEnabledIntegrationTests(SwaggerEnabledWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_WhenEnabled_ReturnsOk()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class SwaggerDisabledIntegrationTests : IClassFixture<SwaggerDisabledWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SwaggerDisabledIntegrationTests(SwaggerDisabledWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_WhenDisabled_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public sealed class SwaggerEnabledWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "integration-tests-signing-key-2026",
                ["ENABLE_SWAGGER"] = "true"
            });
        });
    }
}

public sealed class SwaggerDisabledWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "integration-tests-signing-key-2026",
                ["ENABLE_SWAGGER"] = "false"
            });
        });
    }
}

