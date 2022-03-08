using StackExchange.Redis;

namespace Api.CacheLib;

public static class ServiceExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services,
        Action<RedisConnectionOptions> configAction)
    {
        ArgumentNullException.ThrowIfNull(nameof(services));
        ArgumentNullException.ThrowIfNull(nameof(configAction));
        services.AddOptions<RedisConnectionOptions>();
        services.Configure(configAction);
        services.AddSingleton<IRedisDataStore, RedisDataStore>();
        return services;

    }
   
}