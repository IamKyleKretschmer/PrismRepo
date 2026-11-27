using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class ObservabilityTests
{
    private const string RequestLogCategory = "TodoApi.Api.RequestLoggingMiddleware";
    private const string DistinctiveTitle = "Ring Dr Okafor about the biopsy results";

    [Fact]
    public async Task Unauthenticated_request_is_logged_with_correlation_id_status_and_code()
    {
        var logs = new CapturedLogs();
        await using var factory = NewFactory(logs);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "corr-123");

        var response = await client.GetAsync("/v1/todos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("corr-123", response.Headers.GetValues("X-Correlation-ID").Single());

        var entry = Assert.Single(logs.Entries, e => e.Category == RequestLogCategory && e.Level == LogLevel.Information);
        Assert.Equal("corr-123", entry.Scopes["CorrelationId"]);
        Assert.Equal(401, entry.State["StatusCode"]);
        Assert.Equal("unauthenticated", entry.State["ErrorCode"]);
        Assert.Equal("GET", entry.State["Method"]);
    }

    [Fact]
    public async Task Correlation_id_is_generated_when_none_is_supplied()
    {
        var logs = new CapturedLogs();
        await using var factory = NewFactory(logs);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        var correlationId = response.Headers.GetValues("X-Correlation-ID").Single();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        var entry = Assert.Single(logs.Entries, e => e.Category == RequestLogCategory && e.Level == LogLevel.Information);
        Assert.Equal(correlationId, entry.Scopes["CorrelationId"]);
        Assert.Equal("/health", entry.State["Route"]);
        Assert.Equal(200, entry.State["StatusCode"]);
    }

    [Fact]
    public async Task Unsafe_correlation_id_is_replaced()
    {
        await using var factory = NewFactory(new CapturedLogs());
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "bad id\twith spaces");

        var response = await client.GetAsync("/health");

        Assert.NotEqual("bad id\twith spaces", response.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task No_log_entry_at_any_level_contains_a_token_or_request_body()
    {
        var logs = new CapturedLogs();
        await using var factory = NewFactory(logs);
        var token = TestTokens.Valid();
        var expiredToken = TestTokens.Create(expires: DateTime.UtcNow.AddMinutes(-10));
        var body = $$"""{"title":"{{DistinctiveTitle}}"}""";

        using (var client = factory.CreateClient().Authenticated(token))
        {
            await client.PostAsync("/v1/todos", new StringContent(body, Encoding.UTF8, "application/json"));
            await client.GetAsync("/v1/todos");
        }

        using (var client = factory.CreateClient().Authenticated(expiredToken))
        {
            await client.PostAsync("/v1/todos", new StringContent(body, Encoding.UTF8, "application/json"));
        }

        Assert.Contains(logs.Entries, e => e.Level == LogLevel.Debug);
        var text = logs.AllText;
        Assert.DoesNotContain(DistinctiveTitle, text);
        Assert.DoesNotContain(token, text);
        Assert.DoesNotContain(token.Split('.')[2], text);
        Assert.DoesNotContain(expiredToken.Split('.')[2], text);
        Assert.DoesNotContain(TestTokens.Subject, text);
    }

    [Fact]
    public async Task Caller_reference_appears_only_at_debug_level()
    {
        var logs = new CapturedLogs();
        await using var factory = NewFactory(logs);
        using var client = factory.CreateClient().Authenticated();

        await client.GetAsync("/v1/no-such-route");

        var callerReference = TodoApi.Api.RequestLoggingMiddleware.CallerReference(TestTokens.Subject);
        var mentions = logs.Entries.Where(e => e.AllText.Contains(callerReference, StringComparison.Ordinal)).ToList();
        Assert.NotEmpty(mentions);
        Assert.All(mentions, e => Assert.Equal(LogLevel.Debug, e.Level));
    }

    [Fact]
    public async Task Request_count_and_latency_are_emitted_per_route_and_status()
    {
        await using var factory = new TodoApiFactory();
        using var client = factory.CreateClient();
        using var collector = new MetricCollector<double>(
            factory.Services.GetRequiredService<System.Diagnostics.Metrics.IMeterFactory>(),
            "Microsoft.AspNetCore.Hosting",
            "http.server.request.duration");

        await client.GetAsync("/health");
        await client.GetAsync("/health");
        await collector.WaitForMeasurementsAsync(2, TimeSpan.FromSeconds(5));

        var health = collector.GetMeasurementSnapshot()
            .Where(m => Equals(m.Tags["http.route"], "/health") && Equals(m.Tags["http.response.status_code"], 200))
            .ToList();
        Assert.Equal(2, health.Count);
        Assert.All(health, m => Assert.True(m.Value > 0));
    }

    private static TodoApiFactory NewFactory(CapturedLogs logs) => new(
        settings: CapturedLogs.DebugSettings,
        configureServices: services => services.AddSingleton<ILoggerProvider>(logs));
}
