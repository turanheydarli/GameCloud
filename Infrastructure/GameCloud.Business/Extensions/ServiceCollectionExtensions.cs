using System.Reflection;
using System.Text;
using GameCloud.Application.Common.Mappings;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Users;
using GameCloud.Business.Services;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GameCloud.Business.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IGameContext, GameContextAccessor>();
        services.AddScoped<IGameKeyResolver, GameKeyResolver>();

        services.AddScoped<IGameKeyRepository, GameKeyRepository>();
        services.AddScoped<IDeveloperRepository, DeveloperRepository>();
        services.AddScoped<IDeveloperService, DeveloperService>();
        services.AddScoped<IUserService, UserService>();

        services.AddDbContext<GameCloudDbContext>(opts =>
        {
            opts.UseNpgsql(configuration.GetConnectionString("GameCloud"));
        });

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<GameCloudDbContext>()
            .AddDefaultTokenProviders();

        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };

                cfg.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        throw new UnauthorizedAccessException("Authentication failed. Please provide a valid token.");
                    },
                    OnForbidden = context =>
                        throw new UnauthorizedAccessException("You do not have permission to access this resource.")
                };
            });

        services.AddAutoMapper(Assembly.GetAssembly(typeof(GeneralMappingProfile)));

        return services;
    }
}