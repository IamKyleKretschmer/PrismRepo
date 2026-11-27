using System.Net;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class StartupAndHealthTests
{
    [Theory]
    [InlineData("Storage:DataFilePath")]
    [InlineData("Authentication:Issuer")]
    [InlineData("Authentication:Audience")]
    public async Task Startup_fails_naming_a_missing_setting(string missingSetting)
    {
        await using var factory = new TodoApiFactory(settings: new Dictionary<string, string?> { [missingSetting] = null });

        var message = StartupFailureMessage(factory);

        Assert.Contains(missingSetting, message);
        Assert.DoesNotContain(TestAuth.SigningKey, message);
    }

    [Fact]
    public async Task Startup_fails_when_no_signing_key_source_is_set()
    {
        await using var factory = new TodoApiFactory(settings: new Dictionary<string, string?>
        {
            ["Authentication:SigningKey"] = null,
            ["Authentication:Authority"] = null,
        });

        var message = StartupFailureMessage(factory);

        Assert.Contains("Authentication:SigningKey", message);
        Assert.Contains("Authentication:Authority", message);
    }

    [Fact]
    public async Task Startup_names_every_missing_setting_and_no_secret_value()
    {
        await using var factory = new TodoApiFactory(settings: new Dictionary<string, string?>
        {
            ["Storage:DataFilePath"] = null,
            ["Authentication:Issuer"] = null,
            ["Authentication:SigningKey"] = "dG9vLXNob3J0",
        });

        var message = StartupFailureMessage(factory);

        Assert.Contains("Storage:DataFilePath", message);
        Assert.Contains("Authentication:Issuer", message);
        Assert.Contains("Authentication:SigningKey", message);
        Assert.DoesNotContain("dG9vLXNob3J0", message);
    }

    [Fact]
    public async Task Fresh_path_creates_the_data_file_and_health_is_healthy_without_a_token()
    {
        await using var factory = new TodoApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        Assert.True(File.Exists(factory.DataFilePath));
    }

    [Fact]
    public async Task Health_is_unhealthy_when_the_data_file_is_not_writable()
    {
        var dataFilePath = TestDataFile.NewPath();
        Directory.CreateDirectory(Path.GetDirectoryName(dataFilePath)!);
        await File.WriteAllBytesAsync(dataFilePath, []);
        TestDataFile.MakeReadOnly(dataFilePath);
        try
        {
            await using var factory = new TodoApiFactory(dataFilePath);
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            TestDataFile.Delete(dataFilePath);
        }
    }

    [Fact]
    public async Task Health_is_unhealthy_when_the_data_file_location_is_unreachable()
    {
        var blocker = TestDataFile.NewPath();
        Directory.CreateDirectory(Path.GetDirectoryName(blocker)!);
        await File.WriteAllBytesAsync(blocker, []);
        try
        {
            await using var factory = new TodoApiFactory(Path.Combine(blocker, "nested", "todo.db"));
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            TestDataFile.Delete(blocker);
        }
    }

    private static string StartupFailureMessage(TodoApiFactory factory)
    {
        var exception = Record.Exception(() => factory.CreateClient());
        Assert.NotNull(exception);
        return string.Join(Environment.NewLine, Flatten(exception).Select(e => e.Message));
    }

    private static IEnumerable<Exception> Flatten(Exception exception) =>
        exception is AggregateException aggregate
            ? aggregate.InnerExceptions.SelectMany(Flatten)
            : exception.InnerException is null ? [exception] : [exception, .. Flatten(exception.InnerException)];
}
