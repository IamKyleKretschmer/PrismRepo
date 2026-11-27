using System.Text.Json;
using TodoApi.Items;

namespace TodoApi.Api;

/// <summary>Reads and validates a create request body, never buffering more than the size limit.</summary>
internal static class CreateTodoRequest
{
    public const int MaxBodyBytes = 64 * 1024;

    public sealed record Outcome(string? Title, IResult? Rejection);

    public static async Task<Outcome> ReadAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (context.Request.ContentLength > MaxBodyBytes)
        {
            return Reject(context, TooLarge());
        }

        var body = await ReadBoundedAsync(context.Request.Body, cancellationToken);
        if (body is null)
        {
            return Reject(context, TooLarge());
        }

        var problem = ParseTitle(body, out var title);
        return problem == TitleProblem.None ? new Outcome(title, null) : Reject(context, ForTitle(problem));
    }

    private static TitleProblem ParseTitle(byte[] body, out string? title)
    {
        title = null;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("title", out var element)
                && element.ValueKind == JsonValueKind.String)
            {
                title = element.GetString();
            }
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // Unparseable JSON, or a string holding unpaired UTF-16 surrogates: both count as no usable title.
            title = null;
        }

        return TitleRules.Check(title);
    }

    /// <returns>The body, or null when it exceeds <see cref="MaxBodyBytes"/>.</returns>
    private static async Task<byte[]?> ReadBoundedAsync(Stream body, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;
        while ((read = await body.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaxBodyBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static Outcome Reject(HttpContext context, (int Status, string Code, string Title, string Detail) problem)
    {
        context.Items[ApiProblem.ErrorCodeItemKey] = problem.Code;
        return new Outcome(null, ApiProblem.Create(problem.Status, problem.Code, problem.Title, problem.Detail));
    }

    private static (int, string, string, string) TooLarge() => (
        StatusCodes.Status413PayloadTooLarge,
        ErrorCodes.RequestTooLarge,
        "Request too large",
        $"The request body must be at most {MaxBodyBytes / 1024} KB.");

    private static (int, string, string, string) ForTitle(TitleProblem problem) => problem switch
    {
        TitleProblem.Missing => Invalid(ErrorCodes.TitleMissing, "The request body must be a JSON object with a string \"title\" field."),
        TitleProblem.Empty => Invalid(ErrorCodes.TitleEmpty, "The title must not be empty."),
        TitleProblem.Blank => Invalid(ErrorCodes.TitleBlank, "The title must contain at least one character that isn't whitespace."),
        TitleProblem.TooLong => Invalid(ErrorCodes.TitleTooLong,
            $"The title must be at most {TitleRules.MaxCharacters} characters. Each emoji or accented letter counts as one character, and leading and trailing spaces count."),
        TitleProblem.TooLarge => Invalid(ErrorCodes.TitleTooLarge,
            $"The title must be at most {TitleRules.MaxCodePoints:N0} Unicode code points."),
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, null),
    };

    private static (int, string, string, string) Invalid(string code, string detail) =>
        (StatusCodes.Status400BadRequest, code, "Invalid title", detail);
}
