using TodoApi.Items;

namespace TodoApi.Persistence;

public sealed class TodoStore(TodoDatabase database, TimeProvider clock)
{
    /// <summary>Stores a new item. Creation time and sequence are assigned under the write lock, so they agree.</summary>
    public async Task<TodoItem> CreateAsync(string title, CancellationToken cancellationToken)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(deferred: false);

        var item = new TodoItem(Guid.NewGuid().ToString("N"), title, StoredTime.Now(clock), CompletedAt: null);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO todo_items (id, title, created_at) VALUES ($id, $title, $createdAt);";
        command.Parameters.AddWithValue("$id", item.Id);
        command.Parameters.AddWithValue("$title", item.Title);
        command.Parameters.AddWithValue("$createdAt", StoredTime.Write(item.CreatedAt));
        await command.ExecuteNonQueryAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return item;
    }

    /// <summary>Every item, oldest first; ties on creation time fall back to commit order.</summary>
    public async Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, title, created_at, completed_at FROM todo_items ORDER BY created_at, seq;";

        var items = new List<TodoItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TodoItem(
                reader.GetString(0),
                reader.GetString(1),
                StoredTime.Read(reader.GetString(2)),
                reader.IsDBNull(3) ? null : StoredTime.Read(reader.GetString(3))));
        }

        return items;
    }
}
