namespace TodoApi.Persistence;

internal static class Schema
{
    public const string Create = """
        CREATE TABLE IF NOT EXISTS health_probe (
            id         INTEGER PRIMARY KEY CHECK (id = 1),
            checked_at TEXT NOT NULL
        );
        """;
}
