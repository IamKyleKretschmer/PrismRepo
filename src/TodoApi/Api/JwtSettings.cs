using Microsoft.Extensions.Options;

namespace TodoApi.Api;

/// <summary>Vibranium JWT validation settings. Exactly one signing-key source must be set.</summary>
public sealed class JwtSettings
{
    public const string Section = "Authentication";

    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    /// <summary>Issuer base URL whose OpenID Connect metadata publishes the signing keys. Use in deployed environments.</summary>
    public string? Authority { get; set; }

    /// <summary>Base64 symmetric key of at least 256 bits. For local and test runs only.</summary>
    public string? SigningKey { get; set; }
}

internal sealed class JwtSettingsValidator : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings settings)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.Issuer))
        {
            failures.Add(RequiredSetting.Missing(JwtSettings.Section, nameof(JwtSettings.Issuer)));
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            failures.Add(RequiredSetting.Missing(JwtSettings.Section, nameof(JwtSettings.Audience)));
        }

        var authority = RequiredSetting.Name(JwtSettings.Section, nameof(JwtSettings.Authority));
        var signingKey = RequiredSetting.Name(JwtSettings.Section, nameof(JwtSettings.SigningKey));
        var hasAuthority = !string.IsNullOrWhiteSpace(settings.Authority);
        var hasSigningKey = !string.IsNullOrWhiteSpace(settings.SigningKey);

        if (hasAuthority == hasSigningKey)
        {
            failures.Add(hasAuthority
                ? $"Set only one signing-key source: {authority} or {signingKey}, not both."
                : $"Missing required signing-key source: set {authority} or {signingKey}.");
        }
        else if (hasSigningKey && !IsStrongBase64Key(settings.SigningKey!))
        {
            failures.Add($"{signingKey} must be base64 and at least 256 bits long.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    internal static bool IsStrongBase64Key(string value)
    {
        var buffer = new byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out var length) && length >= 32;
    }
}
