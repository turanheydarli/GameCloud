using GameCloud.Application.Common.Interfaces;
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

        var options = new ConfigurationOptions
        {
            EndPoints = { redisConnection },
            AbortOnConnectFail = false,
            ConnectTimeout = 5000,
            SyncTimeout = 5000
        };

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.ConfigurationOptions = options;
            redisOptions.InstanceName = "GameCloud_";
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(options));

        services.AddSingleton<IMatchStateCache, RedisMatchStateCache>();
        services.AddSingleton<ISessionCache, RedisSessionCache>();

        return services;
    }
}