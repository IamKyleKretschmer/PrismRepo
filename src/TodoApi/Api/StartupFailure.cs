using Microsoft.Extensions.Options;

namespace TodoApi.Api;

internal static class StartupFailure
{
    /// <summary>Describes configuration failures thrown while the host starts; false for anything else.</summary>
    public static bool TryDescribe(Exception exception, out string message)
    {
        var failures = Flatten(exception).OfType<OptionsValidationException>().SelectMany(e => e.Failures).ToList();
        message = failures.Count == 0
            ? string.Empty
            : "The service cannot start because its configuration is incomplete:" + Environment.NewLine
              + string.Join(Environment.NewLine, failures.Select(f => "  - " + f));
        return failures.Count > 0;
    }

    private static IEnumerable<Exception> Flatten(Exception exception) =>
        exception is AggregateException aggregate
            ? aggregate.InnerExceptions.SelectMany(Flatten)
            : [exception];
}
