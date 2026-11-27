using System.Net;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class ServiceHostTests
{
    [Fact]
    public async Task Service_starts_and_stops_cleanly()
    {
        await using var factory = new TodoApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
