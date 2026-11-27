namespace TodoApi.UnitTests;

public class ArchitectureTests
{
    private static readonly string[] ForbiddenInItems =
    [
        "Microsoft.AspNetCore",
        "Microsoft.Data.Sqlite",
        "TodoApi.Api",
        "TodoApi.Persistence",
    ];

    [Fact]
    public void Item_rules_do_not_reference_http_or_storage()
    {
        var itemsDir = Path.Combine(RepoRoot(), "src", "TodoApi", "Items");
        Assert.True(Directory.Exists(itemsDir), $"Missing {itemsDir}");

        var offences =
            from file in Directory.EnumerateFiles(itemsDir, "*.cs", SearchOption.AllDirectories)
            from line in File.ReadLines(file)
            from forbidden in ForbiddenInItems
            where line.Contains(forbidden, StringComparison.Ordinal)
            select $"{Path.GetFileName(file)}: {line.Trim()}";

        Assert.Empty(offences);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TodoApi.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
