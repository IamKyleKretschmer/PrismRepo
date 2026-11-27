using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TodoApi.Persistence;

/// <summary>Healthy only when the data file can be opened and written.</summary>
internal sealed partial class DataStoreHealthCheck(TodoDatabase database, TimeProvider clock, ILogger<DataStoreHealthCheck> logger)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await database.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO health_probe (id, checked_at) VALUES (1, $now)
                ON CONFLICT (id) DO UPDATE SET checked_at = excluded.checked_at;
                """;
            command.Parameters.AddWithValue("$now", clock.GetUtcNow().ToString("O"));
            await command.ExecuteNonQueryAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex) when (StoreFailure.IsStoreFailure(ex))
        {
            LogUnhealthy(logger, ex);
            return HealthCheckResult.Unhealthy("The data store can't be reached or written.");
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Data store health check failed.")]
    private static partial void LogUnhealthy(ILogger logger, Exception exception);
}

internal static class StoreFailure
{
    public static bool IsStoreFailure(Exception ex) =>
        ex is SqliteException or IOException or UnauthorizedAccessException or InvalidOperationException;
}
