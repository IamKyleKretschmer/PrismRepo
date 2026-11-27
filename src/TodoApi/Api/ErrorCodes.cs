namespace TodoApi.Api;

/// <summary>Stable error codes from the v1 contract. Consumers and tests rely on these values.</summary>
public static class ErrorCodes
{
    public const string Unauthenticated = "unauthenticated";
    public const string InternalError = "internal_error";
    public const string TitleMissing = "title_missing";
    public const string TitleEmpty = "title_empty";
    public const string TitleBlank = "title_blank";
    public const string TitleTooLong = "title_too_long";
    public const string TitleTooLarge = "title_too_large";
    public const string RequestTooLarge = "request_too_large";
}
