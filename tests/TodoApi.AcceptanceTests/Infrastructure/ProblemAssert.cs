using System.Net;
using System.Text.Json;

namespace TodoApi.AcceptanceTests.Infrastructure;

public static class ProblemAssert
{
    /// <summary>Asserts a problem-details body with the given status and stable code, and returns it.</summary>
    public static async Task<JsonElement> HasCodeAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = document.RootElement.Clone();
        Assert.Equal(code, body.GetProperty("code").GetString());
        Assert.Equal((int)status, body.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("type").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("detail").GetString()));
        return body;
    }
}
