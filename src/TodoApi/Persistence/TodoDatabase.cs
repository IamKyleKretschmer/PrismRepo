using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace TodoApi.Persistence;

/// <summary>Opens connections to the embedded data file, creating and initialising it on first use.</summary>
public sealed class TodoDatabase
{
    private readonly Lazy<string> _dataFilePath;
    private readonly Lazy<string> _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private volatile bool _initialized;

    // Options are read lazily: hosted services are constructed before startup validation reports missing settings.
    public TodoDatabase(IOptions<StorageOptions> options)
    {
        _dataFilePath = new(() => Path.GetFullPath(options.Value.DataFilePath!));

        // Pooling is off so a stopped host releases the file; restart tests and backups rely on that.
        _connectionString = new(() => new SqliteConnectionStringBuilder
        {
            DataSource = _dataFilePath.Value,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
            DefaultTimeout = 30,
        }.ToString());
    }

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            await InitializeAsync(cancellationToken);
        }

        return await OpenConnectionAsync(cancellationToken);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            var directory = Path.GetDirectoryName(_dataFilePath.Value);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode = WAL;";
            var journalMode = (string?)await command.ExecuteScalarAsync(cancellationToken);
            if (!string.Equals(journalMode, "wal", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The data file could not be switched to WAL mode; it is probably read-only.");
            }

            command.CommandText = Schema.Create;
            await command.ExecuteNonQueryAsync(cancellationToken);

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString.Value);
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA synchronous = FULL;";
            await command.ExecuteNonQueryAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
