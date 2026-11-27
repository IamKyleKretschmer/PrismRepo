using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class AuthenticationTests : IAsyncLifetime
{
    private const string OtherKey = "b3RoZXItc2lnbmluZy1rZXktbm90LXRydXN0ZWQtYnktdGhlLXNlcnZpY2U=";
    private readonly TodoApiFactory _factory = new();

    public static TheoryData<string, string?> RejectedTokens() => new()
    {
        { "missing", null },
        { "expired", TestTokens.Create(expires: DateTime.UtcNow.AddMinutes(-10)) },
        { "wrongly signed", TestTokens.Create(signingKey: OtherKey) },
        { "wrong issuer", TestTokens.Create(issuer: "https://someone-else.test/") },
        { "wrong audience", TestTokens.Create(audience: "another-api") },
        { "no subject", TestTokens.Create(subject: null) },
        { "not a JWT", "not-a-token" },
    };

    [Theory]
    [MemberData(nameof(RejectedTokens))]
    public async Task Rejected_tokens_get_401_unauthenticated(string because, string? token)
    {
        using var client = _factory.CreateClient();
        if (token is not null)
        {
            client.Authenticated(token);
        }

        var response = await client.GetAsync("/v1/todos");

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.Unauthorized, "unauthenticated");
        Assert.True(response.Headers.WwwAuthenticate.Count > 0, because);
    }

    [Fact]
    public async Task Ntx_user_id_header_alone_is_not_an_identity()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("ntx-user-id", "someone");

        var response = await client.GetAsync("/v1/todos");

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.Unauthorized, "unauthenticated");
    }

    [Fact]
    public async Task Ntx_user_id_header_does_not_rescue_a_token_without_subject()
    {
        using var client = _factory.CreateClient().Authenticated(TestTokens.Create(subject: null));
        client.DefaultRequestHeaders.Add("ntx-user-id", "someone");

        var response = await client.GetAsync("/v1/todos");

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.Unauthorized, "unauthenticated");
    }

    [Fact]
    public async Task Valid_token_reaches_v1_routing()
    {
        using var client = _factory.CreateClient().Authenticated();

        var response = await client.GetAsync("/v1/no-such-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_answers_without_a_token()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unhandled_failure_returns_a_generic_500()
    {
        const string secret = "internal detail that must not leak";
        await using var factory = new TodoApiFactory(configureServices: services =>
            services.AddSingleton<IStartupFilter>(new ThrowingEndpointFilter("/v1/boom", secret)));
        using var client = factory.CreateClient().Authenticated();

        var response = await client.GetAsync("/v1/boom");

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.InternalServerError, "internal_error");
        Assert.DoesNotContain(secret, await response.Content.ReadAsStringAsync());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    private sealed class ThrowingEndpointFilter(string path, string message) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.Map(path, branch => branch.Run(_ => throw new InvalidOperationException(message)));
        };
    }
}
