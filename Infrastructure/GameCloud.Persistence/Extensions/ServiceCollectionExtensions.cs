using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GameCloud.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {

        return services;
    }
}