using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class DurableConcurrentCreateTests
{
    [Fact]
    public async Task Created_items_survive_a_restart_with_identical_fields_and_order()
    {
        var dataFilePath = TestDataFile.NewPath();
        try
        {
            string before;
            await using (var first = new TodoApiFactory(dataFilePath))
            {
                using var client = first.CreateClient().Authenticated();
                foreach (var title in new[] { "Buy milk", "Call Sam", "  padded  ", "Купить хлеб", "🎉" })
                {
                    await client.CreateItemAsync(title);
                }

                before = Serialize(await client.ListItemsAsync());
            }

            await using var second = new TodoApiFactory(dataFilePath);
            using var restarted = second.CreateClient().Authenticated();

            var after = await restarted.ListItemsAsync();

            Assert.Equal(5, after.Length);
            Assert.Equal(before, Serialize(after));
        }
        finally
        {
            TestDataFile.Delete(dataFilePath);
        }
    }

    [Fact]
    public async Task Fifty_parallel_creates_give_fifty_distinct_complete_items_in_a_repeatable_order()
    {
        await using var factory = new TodoApiFactory();
        using var client = factory.CreateClient().Authenticated();

        var responses = await Task.WhenAll(Enumerable.Range(0, 50).Select(i => client.CreateAsync($"Parallel {i}")));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        var createdIds = await Task.WhenAll(responses.Select(async r => (await r.ReadJsonAsync()).GetProperty("id").GetString()));
        Assert.Equal(50, createdIds.Distinct().Count());

        var listed = await client.ListItemsAsync();
        Assert.Equal(50, listed.Length);
        Assert.Equal(createdIds.Order(), listed.Ids().Order());
        Assert.All(listed, item =>
        {
            Assert.Equal(["id", "title", "isComplete", "createdAt"], item.PropertyNames());
            Assert.StartsWith("Parallel ", item.GetProperty("title").GetString());
        });

        var createdAt = listed.Select(i => i.GetProperty("createdAt").GetDateTime()).ToList();
        Assert.Equal(createdAt.Order(), createdAt);
        Assert.Equal(listed.Ids(), (await client.ListItemsAsync()).Ids());
        Assert.Equal(listed.Ids(), (await client.ListItemsAsync()).Ids());
    }

    [Fact]
    public async Task Items_sharing_a_creation_time_are_listed_in_the_order_they_were_accepted()
    {
        var now = DateTimeOffset.UtcNow;
        await using var factory = new TodoApiFactory(configureServices: services => services.AddSingleton<TimeProvider>(new FrozenClock(now)));
        using var client = factory.CreateClient().Authenticated();
        var accepted = new List<string?>();
        for (var i = 0; i < 10; i++)
        {
            accepted.Add((await client.CreateItemAsync($"Tie {i}")).GetProperty("id").GetString());
        }

        var listed = await client.ListItemsAsync();

        Assert.Single(listed.Select(i => i.GetProperty("createdAt").GetString()).Distinct());
        Assert.Equal(accepted, listed.Ids());
    }

    [Fact]
    public async Task Parallel_creates_sharing_a_creation_time_keep_one_order_across_listings_and_a_restart()
    {
        var dataFilePath = TestDataFile.NewPath();
        var clock = new FrozenClock(DateTimeOffset.UtcNow);
        try
        {
            string?[] order;
            await using (var first = new TodoApiFactory(dataFilePath, configureServices: s => s.AddSingleton<TimeProvider>(clock)))
            {
                using var client = first.CreateClient().Authenticated();
                await Task.WhenAll(Enumerable.Range(0, 50).Select(i => client.CreateAsync($"Tied parallel {i}")));

                order = (await client.ListItemsAsync()).Ids();
                Assert.Equal(50, order.Length);
                Assert.Equal(order, (await client.ListItemsAsync()).Ids());
            }

            await using var second = new TodoApiFactory(dataFilePath);
            using var restarted = second.CreateClient().Authenticated();

            Assert.Equal(order, (await restarted.ListItemsAsync()).Ids());
        }
        finally
        {
            TestDataFile.Delete(dataFilePath);
        }
    }

    private static string Serialize(JsonElement[] items) => JsonSerializer.Serialize(items);
}
