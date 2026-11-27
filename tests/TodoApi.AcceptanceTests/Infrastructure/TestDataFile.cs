namespace TodoApi.AcceptanceTests.Infrastructure;

public static class TestDataFile
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "todo-api-tests");

    public static string NewPath() => Path.Combine(Root, $"{Guid.NewGuid():N}", "todo.db");

    public static void Delete(string dataFilePath)
    {
        var directory = Path.GetDirectoryName(dataFilePath)!;
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }

        Directory.Delete(directory, recursive: true);
    }

    public static void MakeReadOnly(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            File.SetAttributes(path, FileAttributes.ReadOnly);
        }
        else
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
        }
    }
}
