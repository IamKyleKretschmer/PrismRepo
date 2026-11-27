using System.Net;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class HealthTests
{
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
}
