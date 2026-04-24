using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public sealed class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestsWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "integration-tests-signing-key-2026");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "integration-tests-signing-key-2026"
            });
        });
    }
}

