using System.Globalization;

namespace TodoApi.Persistence;

/// <summary>UTC instants at millisecond precision, stored as fixed-width ISO 8601 text so text order is time order.</summary>
internal static class StoredTime
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public static DateTime Now(TimeProvider clock)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond), DateTimeKind.Utc);
    }

    public static string Write(DateTime utc) => utc.ToString(Format, CultureInfo.InvariantCulture);

    public static DateTime Read(string text) =>
        DateTime.ParseExact(text, Format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
