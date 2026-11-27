using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace TodoApi.AcceptanceTests.Infrastructure;

public static class TestTokens
{
    public const string Subject = "test-user-7f3a";

    public static string Valid() => Create();

    public static string Create(
        string? subject = Subject,
        string issuer = TestAuth.Issuer,
        string audience = TestAuth.Audience,
        string signingKey = TestAuth.SigningKey,
        DateTime? expires = null)
    {
        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(10);
        var claims = subject is null ? new Dictionary<string, object>() : new Dictionary<string, object> { ["sub"] = subject };

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Claims = claims,
            Subject = new ClaimsIdentity(),
            IssuedAt = expiresAt.AddMinutes(-20),
            NotBefore = expiresAt.AddMinutes(-20),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
                SecurityAlgorithms.HmacSha256),
        });
    }

    public static HttpClient Authenticated(this HttpClient client, string? token = null)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token ?? Valid());
        return client;
    }
}
