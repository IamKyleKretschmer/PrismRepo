namespace TodoApi.Persistence;

internal static class Schema
{
    // seq is AUTOINCREMENT so it only grows and is never reused: it records the order creates committed.
    public const string Create = """
        CREATE TABLE IF NOT EXISTS health_probe (
            id         INTEGER PRIMARY KEY CHECK (id = 1),
            checked_at TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS todo_items (
            seq          INTEGER PRIMARY KEY AUTOINCREMENT,
            id           TEXT NOT NULL UNIQUE,
            title        TEXT NOT NULL,
            created_at   TEXT NOT NULL,
            completed_at TEXT NULL
        );

        CREATE INDEX IF NOT EXISTS ix_todo_items_order ON todo_items (created_at, seq);
        """;
}
