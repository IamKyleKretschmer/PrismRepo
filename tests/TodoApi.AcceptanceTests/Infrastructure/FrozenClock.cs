namespace TodoApi.AcceptanceTests.Infrastructure;

/// <summary>A clock that never moves, so every item created shares one creation time.</summary>
public sealed class FrozenClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
