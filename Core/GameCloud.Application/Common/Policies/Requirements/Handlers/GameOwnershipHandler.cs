using System.Security.Claims;
using GameCloud.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GameCloud.Application.Common.Policies.Requirements.Handlers;

public sealed class GameOwnershipHandler(IGameRepository gameRepository, IDeveloperRepository developerRepository)
    : AuthorizationHandler<GameOwnershipRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        GameOwnershipRequirement requirement)
    {
        var userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return;
        }

        var gameId = await GetGameId(httpContext);
        if (gameId == null)
        {
            return;
        }

        var developer = await developerRepository.GetByUserIdAsync(userId);
        if (developer == null)
        {
            return;
        }

        var game = await gameRepository.GetByIdAsync(gameId.Value);
        if (game != null && game.DeveloperId == developer.Id)
        {
            context.Succeed(requirement);
        }
    }

    private async Task<Guid?> GetGameId(HttpContext httpContext)
    {
        if (httpContext.Request.RouteValues.TryGetValue("gameId", out var gameIdValue))
        {
            if (gameIdValue is string gameIdString && Guid.TryParse(gameIdString, out var gameId))
            {
                return gameId;
            }
        }

        if (httpContext.Request.Query.TryGetValue("gameId", out var queryGameId))
        {
            if (Guid.TryParse(queryGameId.ToString(), out var gameId))
            {
                return gameId;
            }
        }

        if (httpContext.Request.Method == "POST" && httpContext.Request.HasFormContentType)
        {
            var form = await httpContext.Request.ReadFormAsync();
            if (form.TryGetValue("gameId", out var formGameId))
            {
                if (Guid.TryParse(formGameId.ToString(), out var gameId))
                {
                    return gameId;
                }
            }
        }
        
        if (httpContext.Request.Method == "DELETE" && httpContext.Request.HasFormContentType)
        {
            var form = await httpContext.Request.ReadFormAsync();
            if (form.TryGetValue("gameId", out var formGameId))
            {
                if (Guid.TryParse(formGameId.ToString(), out var gameId))
                {
                    return gameId;
                }
            }
        }

        return null;
    }
}