namespace TodoApi.Api;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Structured JSON logs to stdout. Request count and latency come from ASP.NET Core's built-in
    /// <c>http.server.request.duration</c> metric, which follows OpenTelemetry semantic conventions.
    /// </summary>
    public static WebApplicationBuilder AddTodoObservability(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        return builder;
    }
}
