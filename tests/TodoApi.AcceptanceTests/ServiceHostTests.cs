using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TodoApi.AcceptanceTests;

public class ServiceHostTests
{
    [Fact]
    public async Task Service_starts_and_stops_cleanly()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
