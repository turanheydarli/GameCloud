using System.Reflection;
using System.Security.Claims;
using System.Text;
using Amazon.S3;
using FirebaseAdmin;
using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Common.Mappings;
using GameCloud.Application.Common.Policies.Requirements;
using GameCloud.Application.Common.Policies.Requirements.Handlers;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.ImageDocuments;
using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Sessions;
using GameCloud.Application.Features.Users;
using GameCloud.Business.Services;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Messaging.Brokers;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Repositories;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        services.AddRepositories()
            .AddServices()
            .AddAuthorizationHandlers()
            .AddStorageServices(configuration)
            .AddDatabaseContext(configuration)
            .AddIdentityServices()
            .AddAuthorizationPolicies()
            .AddJwtAuthentication(configuration)
            .AddAutoMapper(Assembly.GetAssembly(typeof(GeneralMappingProfile)));

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IGameKeyRepository, GameKeyRepository>()
            .AddScoped<IGameRepository, GameRepository>()
            .AddScoped<IDeveloperRepository, DeveloperRepository>()
            .AddScoped<IPlayerRepository, PlayerRepository>()
            .AddScoped<IPlayerAttributeRepository, PlayerAttributeRepository>()
            .AddScoped<ISessionRepository, SessionRepository>()
            .AddScoped<IActionLogRepository, ActionLogRepository>()
            .AddScoped<IImageDocumentRepository, ImageDocumentRepository>()
            .AddScoped<IFunctionRepository, FunctionRepository>()
            .AddScoped<INotificationRepository, NotificationRepository>()        
            .AddScoped<IMatchRepository, MatchRepository>()
            .AddScoped<IMatchmakingQueueRepository, MatchmakingQueueRepository>()
            .AddScoped<IMatchActionRepository, MatchActionRepository>()
            .AddScoped<IStoredMatchRepository, StoredMatchRepository>()
            .AddScoped<IStoredPlayerRepository, StoredPlayerRepository>()
            .AddScoped<IMatchTicketRepository, MatchTicketRepository>();
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IGameContext, GameContextAccessor>()
            .AddScoped<IExecutionContextAccessor, ExecutionContextAccessor>()
            .AddScoped<IGameKeyResolver, GameKeyResolver>()
            .AddScoped<IGameService, GameService>()
            .AddScoped<IDeveloperService, DeveloperService>()
            .AddScoped<IPlayerService, PlayerService>()
            .AddScoped<IPlayerAttributeService, PlayerAttributeService>()
            .AddScoped<IPermissionValidator, PermissionValidator>()
            .AddScoped<ISessionService, SessionService>()
            .AddScoped<IActionService, ActionService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IFunctionService, FunctionService>()
            .AddScoped<IMatchmakingService, MatchmakingService>()
            .AddScoped<INotificationService, NotificationService>()
            .AddScoped<ITokenService, TokenService>();
    }

    private static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services)
    {
        return services
            .AddScoped<IAuthorizationHandler, GameOwnershipHandler>()
            .AddScoped<IAuthorizationHandler, GameKeyRequirementHandler>();
    }

    private static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = configuration.GetValue<string>("Environment") ?? "Production";

        if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            return services
                .AddFirebaseStorage(configuration)
                .AddScoped<IImageService, FirebaseStorageService>();
        }

        return services
            .AddYandexStorage(configuration)
            .AddScoped<IImageService, YandexStorageService>();
    }

    private static IServiceCollection AddFirebaseStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FirebaseStorageOptions>(configuration.GetSection("FirebaseStorage"));

        var credentialsPath = configuration["FirebaseStorage:CredentialsPath"]
                              ?? throw new InvalidOperationException("Firebase credentials path is not configured.");

        var credential = GoogleCredential.FromFile(credentialsPath);
        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = configuration["FirebaseStorage:ProjectId"],
        });

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

        return services;
    }

    private static IServiceCollection AddYandexStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YandexStorageOptions>(configuration.GetSection("YandexStorage"));

        var options = configuration.GetSection("YandexStorage").Get<YandexStorageOptions>()
                      ?? throw new InvalidOperationException("Yandex storage options are not configured.");

        services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(
            options.AccessKey,
            options.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = true
            }));

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddDbContext<GameCloudDbContext>(opts =>
        {
            opts.UseNpgsql(configuration.GetConnectionString("GameCloud"));
        });
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        return services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<GameCloudDbContext>()
            .AddDefaultTokenProviders()
            .Services;
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("OwnsGame", policy =>
            {
                policy.RequireRole("Developer");
                policy.Requirements.Add(new GameOwnershipRequirement());
            });

            options.AddPolicy("HasGameKey", policy => { policy.Requirements.Add(new GameKeyRequirement()); });
        });

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
    
                cfg.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        throw new UnauthorizedAccessException("Authentication failed. Please provide a valid token.");
                    },
                    OnForbidden = context =>
                        throw new UnauthorizedAccessException("You have not access to this operation.")
                };
            });
    
        return services;
    }
    
    public static IServiceCollection AddMessageQueue(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<YandexCloudOptions>(
            configuration.GetSection("YandexCloud:MessageQueue"));

        services.AddSingleton<IEventPublisher, YandexSqsPublisher>();

        return services;
    }
}