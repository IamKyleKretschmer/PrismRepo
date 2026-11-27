using System.Globalization;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class CreateTodoTests : IAsyncLifetime
{
    private readonly TodoApiFactory _factory = new();

    [Fact]
    public async Task Valid_create_returns_201_with_the_new_item()
    {
        using var client = _factory.CreateClient().Authenticated();
        var before = DateTime.UtcNow.AddSeconds(-1);

        var response = await client.CreateAsync("Buy milk");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var item = await response.ReadJsonAsync();
        var id = item.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.Equal("Buy milk", item.GetProperty("title").GetString());
        Assert.False(item.GetProperty("isComplete").GetBoolean());
        Assert.False(item.TryGetProperty("completedAt", out _));

        var createdAtText = item.GetProperty("createdAt").GetString()!;
        Assert.EndsWith("Z", createdAtText);
        var createdAt = DateTime.Parse(createdAtText, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        Assert.InRange(createdAt, before, DateTime.UtcNow.AddSeconds(1));

        Assert.Equal($"/v1/todos/{id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Response_fields_match_the_contract_names_exactly()
    {
        using var client = _factory.CreateClient().Authenticated();

        var item = await client.CreateItemAsync("Contract check");

        Assert.Equal(["id", "title", "isComplete", "createdAt"], item.PropertyNames());
    }

    [Theory]
    [InlineData("Купить молоко")]
    [InlineData("牛乳を買う")]
    [InlineData("🎉 party for 👨‍👩‍👧‍👦 at café")]
    [InlineData("  padded title  ")]
    [InlineData("\tleading tab and trailing newline\n")]
    public async Task Title_comes_back_exactly_as_sent(string title)
    {
        using var client = _factory.CreateClient().Authenticated();

        var item = await client.CreateItemAsync(title);

        Assert.Equal(title, item.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Duplicate_titles_create_distinct_items()
    {
        using var client = _factory.CreateClient().Authenticated();

        var first = await client.CreateItemAsync("Same title");
        var second = await client.CreateItemAsync("Same title");

        Assert.NotEqual(first.GetProperty("id").GetString(), second.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Identifiers_are_opaque_not_sequential_integers()
    {
        using var client = _factory.CreateClient().Authenticated();

        var item = await client.CreateItemAsync("Opaque id");

        Assert.False(long.TryParse(item.GetProperty("id").GetString(), out _));
    }

    [Fact]
    public async Task Create_without_a_valid_token_returns_401_and_creates_nothing()
    {
        using var client = _factory.CreateClient();

        var response = await client.CreateAsync("Should not exist");

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.Unauthorized, "unauthenticated");
        Assert.Equal(0, TodoClient.CountStoredItems(_factory.DataFilePath));
    }

    [Fact]
    public async Task Title_never_appears_in_logs()
    {
        const string title = "Pick up Mrs Patel's insulin";
        var logs = new CapturedLogs();
        await using var factory = new TodoApiFactory(
            settings: CapturedLogs.DebugSettings,
            configureServices: services => services.AddSingleton<ILoggerProvider>(logs));
        using var client = factory.CreateClient().Authenticated();

        await client.CreateItemAsync(title);

        Assert.NotEmpty(logs.Entries);
        Assert.DoesNotContain(title, logs.AllText);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _factory.DisposeAsync();
}
