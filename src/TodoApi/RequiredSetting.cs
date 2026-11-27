namespace TodoApi;

internal static class RequiredSetting
{
    public static string Missing(string section, string key) =>
        $"Missing required setting {Name(section, key)} (environment variable {EnvironmentVariable(section, key)}).";

    public static string Name(string section, string key) => $"{section}:{key}";

    public static string EnvironmentVariable(string section, string key) => $"{section}__{key}";
}
