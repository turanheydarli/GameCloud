using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Matchmakers;
using GameCloud.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GameCloud.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = configuration.GetValue<string>("Environment") ?? "Production";

        if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            InitializeRedisCache(services, configuration);
        }
        else
        {
            InitializeValkeyCache(services, configuration);
        }

        services.AddSingleton<IMatchStateCache, RedisMatchStateCache>();
        services.AddSingleton<ISessionCache, RedisSessionCache>();

        return services;
    }

    private static void InitializeRedisCache(IServiceCollection services, IConfiguration configuration)
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
    }

    private static void InitializeValkeyCache(IServiceCollection services, IConfiguration configuration)
    {
        var valkeyConnection = configuration.GetConnectionString("Valkey")
                               ?? throw new InvalidOperationException("Valkey connection string is not configured.");

        var options = ConfigurationOptions.Parse(valkeyConnection);
    
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 5000;
        options.SyncTimeout = 5000;
        // options.Ssl = true;
        // options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.ConfigurationOptions = options;
            redisOptions.InstanceName = "GameCloud_";
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(options));
    }
}