using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TodoApi.Api;

namespace TodoApi.UnitTests;

public class RequestLoggingTests
{
    private const string Title = "Collect prescription for J. Smith";
    private const string Token = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1c2VyLTEifQ.c2lnbmF0dXJl";

    [Fact]
    public async Task Request_with_a_title_and_a_token_logs_neither()
    {
        var logger = new RecordingLogger();
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/v1/todos";
        context.Request.Headers.Authorization = $"Bearer {Token}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes($$"""{"title":"{{Title}}"}"""));
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-1")], "test"));
        var middleware = new RequestLoggingMiddleware(
            async ctx =>
            {
                using var reader = new StreamReader(ctx.Request.Body);
                await reader.ReadToEndAsync();
                ctx.Response.StatusCode = 201;
            },
            logger);

        await middleware.InvokeAsync(context);

        Assert.NotEmpty(logger.Lines);
        var output = string.Join(Environment.NewLine, logger.Lines);
        Assert.DoesNotContain(Title, output);
        Assert.DoesNotContain(Token, output);
        Assert.DoesNotContain("user-1", output);
        Assert.Contains("201", output);
    }

    [Fact]
    public void Caller_reference_is_stable_and_does_not_reveal_the_subject()
    {
        var reference = RequestLoggingMiddleware.CallerReference("user-1");

        Assert.Equal(reference, RequestLoggingMiddleware.CallerReference("user-1"));
        Assert.NotEqual(reference, RequestLoggingMiddleware.CallerReference("user-2"));
        Assert.DoesNotContain("user-1", reference);
        Assert.Equal(16, reference.Length);
    }

    private sealed class RecordingLogger : ILogger<RequestLoggingMiddleware>
    {
        public List<string> Lines { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            Lines.Add($"scope: {Describe(state)}");
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Lines.Add($"{logLevel}: {formatter(state, exception)} {Describe(state)}");

        private static string Describe(object? state) =>
            state is IEnumerable<KeyValuePair<string, object?>> pairs
                ? string.Join(", ", pairs.Select(kv => $"{kv.Key}={kv.Value}"))
                : state is IEnumerable<KeyValuePair<string, object>> objectPairs
                    ? string.Join(", ", objectPairs.Select(kv => $"{kv.Key}={kv.Value}"))
                    : state?.ToString() ?? string.Empty;
    }
}
