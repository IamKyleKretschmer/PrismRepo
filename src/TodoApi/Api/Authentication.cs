using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TodoApi.Api;

/// <summary>
/// Vibranium JWT bearer authentication. Identity comes only from the <c>sub</c> claim; the
/// <c>ntx-user-id</c> header is never read because no approved trusted path fronts this service.
/// </summary>
internal static class Authentication
{
    public const string SubjectClaim = "sub";

    public static IServiceCollection AddTodoAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, settings) => Configure(options, settings.Value));

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }

    private static void Configure(JwtBearerOptions options, JwtSettings settings)
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = SubjectClaim,
        };

        if (!string.IsNullOrWhiteSpace(settings.Authority))
        {
            options.Authority = settings.Authority;
            options.RequireHttpsMetadata = true;
        }
        else
        {
            options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(settings.SigningKey!));
            options.TokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.HmacSha256];
        }

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Principal?.FindFirst(SubjectClaim)?.Value))
                {
                    context.Fail("The token has no subject.");
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
                await ApiProblem.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    ErrorCodes.Unauthenticated,
                    "Authentication required",
                    "Send a valid bearer token in the Authorization header.");
            },
        };
    }
}
