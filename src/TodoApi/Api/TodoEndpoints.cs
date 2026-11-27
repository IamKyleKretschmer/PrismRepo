using System.Text.Json;
using TodoApi.Persistence;

namespace TodoApi.Api;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var todos = app.MapGroup("/v1/todos").RequireAuthorization();

        todos.MapPost("", CreateAsync);
        todos.MapGet("", ListAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(TodoStore store, CancellationToken cancellationToken)
    {
        var items = await store.ListAsync(cancellationToken);
        return TypedResults.Ok(new TodoListResponse(items.Select(TodoItemResponse.From).ToList()));
    }

    private static async Task<IResult> CreateAsync(HttpRequest request, TodoStore store, CancellationToken cancellationToken)
    {
        var title = await ReadTitleAsync(request, cancellationToken);
        if (title is null)
        {
            return Problem(request.HttpContext, StatusCodes.Status400BadRequest, ErrorCodes.TitleMissing, "Title missing",
                "The request body must be a JSON object with a string \"title\" field.");
        }

        var item = await store.CreateAsync(title, cancellationToken);
        return TypedResults.Created($"/v1/todos/{item.Id}", TodoItemResponse.From(item));
    }

    private static async Task<string?> ReadTitleAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
            return document.RootElement.ValueKind == JsonValueKind.Object
                   && document.RootElement.TryGetProperty("title", out var title)
                   && title.ValueKind == JsonValueKind.String
                ? title.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IResult Problem(HttpContext context, int status, string code, string title, string detail)
    {
        context.Items[ApiProblem.ErrorCodeItemKey] = code;
        return ApiProblem.Create(status, code, title, detail);
    }
}
