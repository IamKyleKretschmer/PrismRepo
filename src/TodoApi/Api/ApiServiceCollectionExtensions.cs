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

        return services;
    }
}
