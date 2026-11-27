using System.Text.Json.Serialization;
using TodoApi.Items;

namespace TodoApi.Api;

/// <summary>The v1 contract's item representation.</summary>
public sealed record TodoItemResponse(
    string Id,
    string Title,
    bool IsComplete,
    DateTime CreatedAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTime? CompletedAt)
{
    public static TodoItemResponse From(TodoItem item) =>
        new(item.Id, item.Title, item.IsComplete, item.CreatedAt, item.CompletedAt);
}

public sealed record TodoListResponse(IReadOnlyList<TodoItemResponse> Items);
