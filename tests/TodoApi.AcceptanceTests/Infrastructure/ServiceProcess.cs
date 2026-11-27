using System.Diagnostics;

namespace TodoApi.AcceptanceTests.Infrastructure;

/// <summary>Runs the built service as a real process, for behaviour that only a process shows (exit codes, stderr).</summary>
public static class ServiceProcess
{
    public sealed record Result(int ExitCode, string StandardOutput, string StandardError);

    /// <param name="settings">Environment-variable settings; null removes one. Other service settings are cleared.</param>
    public static async Task<Result> RunUntilExitAsync(IReadOnlyDictionary<string, string?> settings, TimeSpan timeout)
    {
        var startInfo = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = AppContext.BaseDirectory,
        };
        startInfo.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "TodoApi.dll"));

        foreach (var key in startInfo.Environment.Keys.Where(IsServiceSetting).ToList())
        {
            startInfo.Environment.Remove(key);
        }

        startInfo.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:0";
        foreach (var (key, value) in settings)
        {
            if (value is null)
            {
                startInfo.Environment.Remove(key);
            }
            else
            {
                startInfo.Environment[key] = value;
            }
        }

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"The service did not exit within {timeout}.");
        }

        return new Result(process.ExitCode, await stdout, await stderr);
    }

    private static bool IsServiceSetting(string key) =>
        key.StartsWith("Storage__", StringComparison.OrdinalIgnoreCase)
        || key.StartsWith("Authentication__", StringComparison.OrdinalIgnoreCase);
}
