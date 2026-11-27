using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApi.AcceptanceTests.Infrastructure;

namespace TodoApi.AcceptanceTests;

public class RejectInvalidTitleTests : IAsyncLifetime
{
    private const int BodyLimit = 64 * 1024;
    private const string CombiningAcute = "\u0301";
    private const string ComplexEmoji = "\U0001F468\U0001F3FB\u200D\U0001F469\U0001F3FC\u200D\U0001F467\U0001F3FD\u200D\U0001F466\U0001F3FE";

    private readonly TodoApiFactory _factory = new();

    public static TheoryData<string, string, string> RejectedBodies() => new()
    {
        { "no title field", """{"name":"Buy milk"}""", "title_missing" },
        { "null title", """{"title":null}""", "title_missing" },
        { "numeric title", """{"title":42}""", "title_missing" },
        { "array body", """["Buy milk"]""", "title_missing" },
        { "not JSON", "title=Buy milk", "title_missing" },
        { "empty body", "", "title_missing" },
        { "unpaired surrogate", """{"title":"\ud800"}""", "title_missing" },
        { "empty title", """{"title":""}""", "title_empty" },
        { "spaces only", """{"title":"     "}""", "title_blank" },
        { "tabs only", """{"title":"\t\t"}""", "title_blank" },
        { "newlines only", """{"title":"\n\r\n"}""", "title_blank" },
        { "201 characters", Json(new string('a', 201)), "title_too_long" },
        { "201 complex emoji", Json(string.Concat(Enumerable.Repeat(ComplexEmoji, 201))), "title_too_long" },
        { "4,001 code points in one character", Json("a" + string.Concat(Enumerable.Repeat(CombiningAcute, 4000))), "title_too_large" },
    };

    public static TheoryData<string, string> AcceptedTitles() => new()
    {
        { "1 character", "a" },
        { "200 characters", new string('a', 200) },
        { "200 characters mixing emoji and accented letters", string.Concat(Enumerable.Repeat("é😀ñ👍🏽", 50)) },
        { "200 characters with leading and trailing spaces", " " + new string('a', 198) + " " },
        { "200 complex emoji", string.Concat(Enumerable.Repeat(ComplexEmoji, 200)) },
        { "4,000 code points in one character", "a" + string.Concat(Enumerable.Repeat(CombiningAcute, 3999)) },
    };

    [Theory]
    [MemberData(nameof(RejectedBodies))]
    public async Task Rejected_create_returns_400_with_the_rule_code_and_changes_nothing(string because, string body, string code)
    {
        using var client = _factory.CreateClient().Authenticated();

        var response = await client.PostJsonAsync(body);

        var problem = await ProblemAssert.HasCodeAsync(response, HttpStatusCode.BadRequest, code);
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("detail").GetString()), because);
        Assert.Empty(await client.ListItemsAsync());
    }

    [Theory]
    [MemberData(nameof(AcceptedTitles))]
    public async Task Accepted_boundary_creates_the_item(string because, string title)
    {
        using var client = _factory.CreateClient().Authenticated();

        var response = await client.CreateAsync(title);

        Assert.True(response.StatusCode == HttpStatusCode.Created, because);
        Assert.Equal(title, (await response.ReadJsonAsync()).GetProperty("title").GetString());
    }

    [Fact]
    public async Task Body_at_the_64KB_limit_is_processed_normally()
    {
        using var client = _factory.CreateClient().Authenticated();
        var body = PaddedBody("Buy milk", BodyLimit);

        var response = await client.PostJsonAsync(body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Body_over_64KB_returns_413_and_changes_nothing()
    {
        using var client = _factory.CreateClient().Authenticated();

        var response = await client.PostJsonAsync(PaddedBody("Buy milk", BodyLimit + 1));

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.RequestEntityTooLarge, "request_too_large");
        Assert.Empty(await client.ListItemsAsync());
    }

    [Fact]
    public async Task Chunked_body_over_64KB_returns_413_without_a_content_length()
    {
        using var client = _factory.CreateClient().Authenticated();
        var bytes = Encoding.UTF8.GetBytes(PaddedBody("Buy milk", BodyLimit * 4));
        using var content = new StreamContent(new MemoryStream(bytes));
        content.Headers.ContentType = new("application/json");
        content.Headers.ContentLength = null;

        var response = await client.PostAsync("/v1/todos", content);

        await ProblemAssert.HasCodeAsync(response, HttpStatusCode.RequestEntityTooLarge, "request_too_large");
        Assert.Empty(await client.ListItemsAsync());
    }

    [Fact]
    public async Task Detail_and_logs_never_contain_the_supplied_title()
    {
        var title = "Ask Dr Novak about " + new string('x', 200);
        var logs = new CapturedLogs();
        await using var factory = new TodoApiFactory(
            settings: CapturedLogs.DebugSettings,
            configureServices: services => services.AddSingleton<ILoggerProvider>(logs));
        using var client = factory.CreateClient().Authenticated();

        var response = await client.CreateAsync(title);

        var problem = await ProblemAssert.HasCodeAsync(response, HttpStatusCode.BadRequest, "title_too_long");
        Assert.DoesNotContain("Dr Novak", problem.GetRawText());
        Assert.DoesNotContain("Dr Novak", logs.AllText);
        Assert.Contains(logs.Entries, e => Equals(e.State.GetValueOrDefault("ErrorCode"), "title_too_long"));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    private static string Json(string title) => JsonSerializer.Serialize(new { title });

    /// <summary>A valid JSON body padded with insignificant whitespace to exactly <paramref name="totalBytes"/> bytes.</summary>
    private static string PaddedBody(string title, int totalBytes)
    {
        var core = Json(title);
        return core[..^1] + new string(' ', totalBytes - Encoding.UTF8.GetByteCount(core)) + "}";
    }
}
