using System.Reflection;
using System.Text;
using Amazon.S3;
using FirebaseAdmin;
using GameCloud.Application.Common.Mappings;
using GameCloud.Application.Common.Policies.Requirements;
using GameCloud.Application.Common.Policies.Requirements.Handlers;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.ImageDocuments;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Sessions;
using GameCloud.Application.Features.Users;
using GameCloud.Business.Services;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
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
        services.AddScoped<IGameContext, GameContextAccessor>();
        services.AddScoped<IGameKeyResolver, GameKeyResolver>();
        services.AddScoped<IGameKeyRepository, GameKeyRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameService, GameService>();

        services.AddScoped<IDeveloperRepository, DeveloperRepository>();
        services.AddScoped<IDeveloperService, DeveloperService>();

        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IPlayerService, PlayerService>();

        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionService, SessionService>();

        services.AddScoped<IActionLogRepository, ActionLogRepository>();
        services.AddScoped<IActionService, ActionService>();

        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IAuthorizationHandler, GameOwnershipHandler>();
        services.AddScoped<IAuthorizationHandler, GameKeyRequirementHandler>();

        services.AddScoped<IImageService, YandexStorageService>();
        services.AddScoped<IImageDocumentRepository, ImageDocumentRepository>();

        services.Configure<FirebaseStorageOptions>(configuration.GetSection("FirebaseStorage"));

        var credentialsPath = configuration["FirebaseStorage:CredentialsPath"];
        if (string.IsNullOrEmpty(credentialsPath))
        {
            throw new InvalidOperationException("Firebase credentials path is not configured.");
        }

        var credential = GoogleCredential.FromFile(credentialsPath);
        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = configuration["FirebaseStorage:ProjectId"],
        });

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
            configuration["FirebaseStorage:CredentialsPath"]);

        services.Configure<YandexStorageOptions>(
            configuration.GetSection("YandexStorage"));

        var options = configuration.GetSection("YandexStorage")
            .Get<YandexStorageOptions>();

        services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(
            options.AccessKey,
            options.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = true
            }));


        
        services.AddScoped<IFunctionRepository, FunctionRepository>();
        services.AddScoped<IFunctionService, FunctionService>();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

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

        services.AddAuthorization(options =>
        {
            options.AddPolicy("OwnsGame", policy =>
            {
                policy.RequireRole("Developer");
                policy.Requirements.Add(new GameOwnershipRequirement());
            });

            options.AddPolicy("HasGameKey", policy =>
            {
                // policy.RequireRole("Player");
                policy.Requirements.Add(new GameKeyRequirement());
            });
        });

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
                        throw new UnauthorizedAccessException("You have not access to this operation.")
                };
            });

        services.AddAutoMapper(Assembly.GetAssembly(typeof(GeneralMappingProfile)));

        return services;
    }
}