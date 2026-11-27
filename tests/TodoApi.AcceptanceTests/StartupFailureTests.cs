using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class StartupFailureTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    [Theory]
    [InlineData("Storage__DataFilePath", "Storage:DataFilePath")]
    [InlineData("Authentication__Issuer", "Authentication:Issuer")]
    [InlineData("Authentication__Audience", "Authentication:Audience")]
    public async Task Process_exits_non_zero_naming_a_missing_setting_and_no_secret(string variable, string setting)
    {
        var settings = CompleteSettings();
        settings[variable] = null;

        var result = await ServiceProcess.RunUntilExitAsync(settings, Timeout);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains(setting, result.StandardError);
        Assert.DoesNotContain(TestAuth.SigningKey, result.StandardError + result.StandardOutput);
    }

    [Fact]
    public async Task Process_exits_non_zero_when_no_signing_key_source_is_set()
    {
        var settings = CompleteSettings();
        settings["Authentication__SigningKey"] = null;

        var result = await ServiceProcess.RunUntilExitAsync(settings, Timeout);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Authentication:SigningKey", result.StandardError);
        Assert.Contains("Authentication:Authority", result.StandardError);
    }

    [Fact]
    public async Task Process_names_every_problem_at_once_without_printing_values()
    {
        const string weakKey = "dG9vLXNob3J0";
        var settings = CompleteSettings();
        settings["Storage__DataFilePath"] = null;
        settings["Authentication__Issuer"] = null;
        settings["Authentication__SigningKey"] = weakKey;

        var result = await ServiceProcess.RunUntilExitAsync(settings, Timeout);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Storage:DataFilePath", result.StandardError);
        Assert.Contains("Authentication:Issuer", result.StandardError);
        Assert.Contains("Authentication:SigningKey", result.StandardError);
        Assert.DoesNotContain(weakKey, result.StandardError + result.StandardOutput);
    }

    private static Dictionary<string, string?> CompleteSettings() => new()
    {
        ["Storage__DataFilePath"] = TestDataFile.NewPath(),
        ["Authentication__Issuer"] = TestAuth.Issuer,
        ["Authentication__Audience"] = TestAuth.Audience,
        ["Authentication__SigningKey"] = TestAuth.SigningKey,
    };
}
