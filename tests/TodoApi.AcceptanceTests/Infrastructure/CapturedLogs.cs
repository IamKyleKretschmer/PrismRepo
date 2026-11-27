using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TodoApi.AcceptanceTests.Infrastructure;

/// <summary>Captures every log entry the host writes, with its structured state and scopes.</summary>
public sealed class CapturedLogs : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentQueue<Entry> _entries = new();
    private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();

    public IReadOnlyCollection<Entry> Entries => _entries;

    /// <summary>Settings that turn every category up to Debug.</summary>
    public static IReadOnlyDictionary<string, string?> DebugSettings { get; } = new Dictionary<string, string?>
    {
        ["Logging:LogLevel:Default"] = "Debug",
        ["Logging:LogLevel:Microsoft"] = "Debug",
        ["Logging:LogLevel:Microsoft.AspNetCore"] = "Debug",
        ["Logging:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics"] = "Debug",
    };

    /// <summary>All text a log sink could see: message, structured values, scopes and exceptions.</summary>
    public string AllText => string.Join(Environment.NewLine, _entries.Select(e => e.AllText));

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, this);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;

    public void Dispose()
    {
    }

    public sealed record Entry(
        string Category,
        LogLevel Level,
        int EventId,
        string Message,
        IReadOnlyDictionary<string, object?> State,
        IReadOnlyDictionary<string, object?> Scopes,
        string? Exception)
    {
        public string AllText => string.Join(
            " | ",
            [Category, Message, .. State.Select(kv => $"{kv.Key}={kv.Value}"), .. Scopes.Select(kv => $"{kv.Key}={kv.Value}"), Exception ?? string.Empty]);
    }

    private sealed class CapturingLogger(string category, CapturedLogs owner) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => owner._scopes.Push(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var values = new Dictionary<string, object?>();
            if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
            {
                foreach (var (key, value) in pairs)
                {
                    values[key] = value;
                }
            }

            var scopes = new Dictionary<string, object?>();
            owner._scopes.ForEachScope(
                (scope, target) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object?>> scopePairs)
                    {
                        foreach (var (key, value) in scopePairs)
                        {
                            target[key] = value;
                        }
                    }
                    else if (scope is IEnumerable<KeyValuePair<string, object>> objectPairs)
                    {
                        foreach (var (key, value) in objectPairs)
                        {
                            target[key] = value;
                        }
                    }
                    else if (scope is not null)
                    {
                        target[$"scope{target.Count}"] = scope.ToString();
                    }
                },
                scopes);

            owner._entries.Enqueue(new Entry(category, logLevel, eventId.Id, formatter(state, exception), values, scopes, exception?.ToString()));
        }
    }
}
