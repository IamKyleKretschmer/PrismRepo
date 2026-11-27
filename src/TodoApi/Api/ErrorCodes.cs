namespace TodoApi.Api;

/// <summary>Stable error codes from the v1 contract. Consumers and tests rely on these values.</summary>
public static class ErrorCodes
{
    public const string Unauthenticated = "unauthenticated";
    public const string InternalError = "internal_error";
    public const string TitleMissing = "title_missing";
}
