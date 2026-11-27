using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace TodoApi.AcceptanceTests.Infrastructure;

public static class TodoClient
{
    public static Task<HttpResponseMessage> CreateAsync(this HttpClient client, string title) =>
        client.PostJsonAsync(JsonSerializer.Serialize(new { title }));

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string json) =>
        client.PostAsync("/v1/todos", new StringContent(json, Encoding.UTF8, "application/json"));

    public static async Task<JsonElement> CreateItemAsync(this HttpClient client, string title)
    {
        var response = await client.CreateAsync(title);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadJsonAsync();
    }

    public static async Task<JsonElement[]> ListItemsAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/v1/todos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.Equal(["items"], body.PropertyNames());
        return body.GetProperty("items").EnumerateArray().ToArray();
    }

    public static string?[] Ids(this IEnumerable<JsonElement> items) =>
        items.Select(i => i.GetProperty("id").GetString()).ToArray();

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    public static string[] PropertyNames(this JsonElement element) =>
        element.EnumerateObject().Select(p => p.Name).ToArray();

    /// <summary>Counts stored items directly, for operations that can't yet be observed through the API.</summary>
    public static int CountStoredItems(string dataFilePath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = dataFilePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false,
        }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM todo_items;";
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
