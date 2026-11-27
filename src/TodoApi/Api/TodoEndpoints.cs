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

    private static async Task<IResult> CreateAsync(HttpContext context, TodoStore store, CancellationToken cancellationToken)
    {
        var request = await CreateTodoRequest.ReadAsync(context, cancellationToken);
        if (request.Rejection is not null)
        {
            return request.Rejection;
        }

        var item = await store.CreateAsync(request.Title!, cancellationToken);
        return TypedResults.Created($"/v1/todos/{item.Id}", TodoItemResponse.From(item));
    }

    private static async Task<IResult> ListAsync(TodoStore store, CancellationToken cancellationToken)
    {
        var items = await store.ListAsync(cancellationToken);
        return TypedResults.Ok(new TodoListResponse(items.Select(TodoItemResponse.From).ToList()));
    }
}
