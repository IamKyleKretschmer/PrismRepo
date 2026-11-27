namespace TodoApi.Api;

/// <summary>Writes the contract's RFC 9457 problem-details error body with its stable <c>code</c>.</summary>
public static class ApiProblem
{
    public const string ErrorCodeItemKey = "todo-api.error-code";

    public static Task WriteAsync(HttpContext context, int status, string code, string title, string detail)
    {
        context.Items[ErrorCodeItemKey] = code;
        return Create(status, code, title, detail).ExecuteAsync(context);
    }

    public static IResult Create(int status, string code, string title, string detail) =>
        TypedResults.Problem(
            detail: detail,
            statusCode: status,
            title: title,
            type: $"urn:todo-api:problem:{code}",
            extensions: new Dictionary<string, object?> { ["code"] = code });
}
