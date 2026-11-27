namespace TodoApi.Persistence;

/// <summary>Creates the data file at startup. A failure is logged rather than thrown so /health can report it.</summary>
internal sealed partial class DatabaseInitializer(TodoDatabase database, ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await database.OpenAsync(cancellationToken);
        }
        catch (Exception ex) when (StoreFailure.IsStoreFailure(ex))
        {
            LogInitializationFailed(logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Error, Message = "The data store could not be opened for writing; /health reports unhealthy until this is fixed.")]
    private static partial void LogInitializationFailed(ILogger logger, Exception exception);
}
