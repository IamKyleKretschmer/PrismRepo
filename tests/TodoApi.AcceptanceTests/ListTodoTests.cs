using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class ListTodoTests : IAsyncLifetime
{
    private readonly TodoApiFactory _factory = new();

    [Fact]
    public async Task Fresh_instance_returns_an_empty_items_array()
    {
        using var client = _factory.CreateClient().Authenticated();

        var items = await client.ListItemsAsync();

        Assert.Empty(items);
    }

    [Fact]
    public async Task Lists_every_item_oldest_first_with_all_contract_fields()
    {
        using var client = _factory.CreateClient().Authenticated();
        var created = new List<string?>();
        foreach (var title in new[] { "Buy milk", "Call Sam", "Book dentist", "Water plants" })
        {
            created.Add((await client.CreateItemAsync(title)).GetProperty("id").GetString());
        }

        var items = await client.ListItemsAsync();

        Assert.Equal(created, items.Ids());
        Assert.Equal(["Buy milk", "Call Sam", "Book dentist", "Water plants"], items.Select(i => i.GetProperty("title").GetString()));
        Assert.All(items, item =>
        {
            Assert.Equal(["id", "title", "isComplete", "createdAt"], item.PropertyNames());
            Assert.False(item.GetProperty("isComplete").GetBoolean());
        });
        var createdAt = items.Select(i => i.GetProperty("createdAt").GetDateTime()).ToList();
        Assert.Equal(createdAt.Order(), createdAt);
    }

    [Fact]
    public async Task Repeated_listings_return_the_same_order()
    {
        using var client = _factory.CreateClient().Authenticated();
        for (var i = 0; i < 20; i++)
        {
            await client.CreateItemAsync($"Item {i}");
        }

        var first = (await client.ListItemsAsync()).Ids();
        var second = (await client.ListItemsAsync()).Ids();
        var third = (await client.ListItemsAsync()).Ids();

        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public async Task Listing_a_few_hundred_items_is_under_200ms_at_p95()
    {
        using var client = _factory.CreateClient().Authenticated();
        await Task.WhenAll(Enumerable.Range(0, 300).Select(i => client.CreateAsync($"Load item {i}")));
        await client.ListItemsAsync();

        var timings = new List<double>();
        for (var i = 0; i < 100; i++)
        {
            var started = Stopwatch.GetTimestamp();
            var response = await client.GetAsync("/v1/todos");
            await response.Content.ReadAsByteArrayAsync();
            timings.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        Assert.Equal(300, (await client.ListItemsAsync()).Length);
        var p95 = timings.Order().ElementAt(94);
        Assert.True(p95 < 200, $"p95 was {p95:0.0} ms");
    }

    [Fact]
    public async Task List_without_a_valid_token_returns_401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/v1/todos");

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.Unauthorized, "unauthenticated");
    }

    [Fact]
    public async Task Titles_never_appear_in_logs_when_listing()
    {
        const string title = "Renew Alex Chen's visa";
        var logs = new CapturedLogs();
        await using var factory = new TodoApiFactory(
            settings: CapturedLogs.DebugSettings,
            configureServices: services => services.AddSingleton<ILoggerProvider>(logs));
        using var client = factory.CreateClient().Authenticated();
        await client.CreateItemAsync(title);

        var items = await client.ListItemsAsync();

        Assert.Single(items);
        Assert.DoesNotContain(title, logs.AllText);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _factory.DisposeAsync();
}
