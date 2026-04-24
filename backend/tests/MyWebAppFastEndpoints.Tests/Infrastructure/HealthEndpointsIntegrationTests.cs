using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class HealthEndpointsIntegrationTests : IClassFixture<TestsWebApplicationFactory>
{
	private readonly HttpClient _client;

	public HealthEndpointsIntegrationTests(TestsWebApplicationFactory factory)
	{
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task Liveness_ReturnsOk()
	{
		var response = await _client.GetAsync("/health/live");
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task Readiness_WhenDatabaseIsReachable_ReturnsOk()
	{
		var response = await _client.GetAsync("/health/ready");
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}
}

public class HealthEndpointsReadinessUnhealthyTests : IClassFixture<UnreadyWebApplicationFactory>
{
	private readonly HttpClient _client;

	public HealthEndpointsReadinessUnhealthyTests(UnreadyWebApplicationFactory factory)
	{
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task Readiness_WhenDatabaseIsUnavailable_ReturnsServiceUnavailable()
	{
		var response = await _client.GetAsync("/health/ready");
		Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
	}
}

public sealed class UnreadyWebApplicationFactory : WebApplicationFactory<Program>
{
	public UnreadyWebApplicationFactory()
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
				["Jwt:SigningKey"] = "integration-tests-signing-key-2026",
				// Read-only mode with a random missing file path forces CanConnect to fail.
				["ConnectionStrings:Default"] = $"Data Source={Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing-health.db")};Mode=ReadOnly"
			});
		});

		// Force readiness to unhealthy deterministically in this test host.
		builder.ConfigureServices(services =>
		{
			services.AddHealthChecks()
				.AddCheck("forced-unready", () => HealthCheckResult.Unhealthy("Forced test failure"), tags: ["ready"]);
		});
	}
}

