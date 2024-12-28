using GameCloud.Application.Features.Functions;
using GameCloud.Functioning.Functions;
using Microsoft.Extensions.DependencyInjection;

namespace GameCloud.Functioning.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddScriptingServices(this IServiceCollection services)
    {
        services.AddHttpClient("Functions");
        services.AddScoped<IFunctionExecutor, HttpFunctionExecutor>();
    }
}