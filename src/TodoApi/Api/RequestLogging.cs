using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TodoApi.Api;

/// <summary>
/// One structured log entry per request, inside a correlation-ID scope. Only the method, route
/// template, status, timing and error code are logged: never bodies, headers, tokens or raw paths.
/// </summary>
public sealed partial class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public const string CorrelationHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        using var scope = logger.BeginScope("CorrelationId:{CorrelationId}", correlationId);
        var started = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "(unmatched)";
            var status = context.Response.StatusCode;

            if (context.Items.TryGetValue(ApiProblem.ErrorCodeItemKey, out var code) && code is string errorCode)
            {
                LogFailed(logger, context.Request.Method, route, status, errorCode, elapsedMs);
            }
            else
            {
                LogCompleted(logger, context.Request.Method, route, status, elapsedMs);
            }

            if (logger.IsEnabled(LogLevel.Debug) && context.User.FindFirst(Authentication.SubjectClaim)?.Value is { } subject)
            {
                LogCaller(logger, CallerReference(subject));
            }
        }
    }

    /// <summary>A stable, non-reversible reference to the caller, so requests can be correlated without logging the identity.</summary>
    public static string CallerReference(string subject) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(subject)))[..16];

    private static string ResolveCorrelationId(HttpContext context)
    {
        var supplied = context.Request.Headers[CorrelationHeader].ToString();
        if (SafeCorrelationId().IsMatch(supplied))
        {
            return supplied;
        }

        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,64}$")]
    private static partial Regex SafeCorrelationId();

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "HTTP {Method} {Route} responded {StatusCode} in {ElapsedMilliseconds:0.0} ms")]
    private static partial void LogCompleted(ILogger logger, string method, string route, int statusCode, double elapsedMilliseconds);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "HTTP {Method} {Route} responded {StatusCode} {ErrorCode} in {ElapsedMilliseconds:0.0} ms")]
    private static partial void LogFailed(ILogger logger, string method, string route, int statusCode, string errorCode, double elapsedMilliseconds);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Caller {CallerReference}")]
    private static partial void LogCaller(ILogger logger, string callerReference);
}
