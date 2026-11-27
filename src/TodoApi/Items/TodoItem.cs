namespace TodoApi.Items;

/// <summary>A to-do item. Completion status is derived from <see cref="CompletedAt"/>, never stored separately.</summary>
public sealed record TodoItem(string Id, string Title, DateTime CreatedAt, DateTime? CompletedAt)
{
    public bool IsComplete => CompletedAt is not null;
}
