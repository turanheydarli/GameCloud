using GameCloud.Application.Features.Matchmakers;
using GameCloud.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GameCloud.Caching.Extensions;

public static class ServiceCollectionExtensions 
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis") 
                              ?? throw new InvalidOperationException("Redis connection string is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "GameCloud_";
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddSingleton<IMatchStateCache, RedisMatchStateCache>();

        return services;
    }

}