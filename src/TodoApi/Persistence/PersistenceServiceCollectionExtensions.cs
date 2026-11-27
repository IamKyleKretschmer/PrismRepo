using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace TodoApi.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddTodoPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TodoDatabase>();
        services.AddHostedService<DatabaseInitializer>();
        services.AddHealthChecks().AddCheck<DataStoreHealthCheck>("data-store");

        return services;
    }
}
