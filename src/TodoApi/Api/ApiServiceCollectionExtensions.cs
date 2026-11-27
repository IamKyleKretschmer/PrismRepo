using Microsoft.Extensions.Options;

namespace TodoApi.Api;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddTodoApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();

        services.AddProblemDetails();
        services.AddTodoAuthentication();

        return services;
    }

    public static WebApplication UseTodoApi(this WebApplication app)
    {
        app.UseExceptionHandler(handler => handler.Run(context => ApiProblem.WriteAsync(
            context,
            StatusCodes.Status500InternalServerError,
            ErrorCodes.InternalError,
            "Internal server error",
            "The request could not be completed. Try again later.")));

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
