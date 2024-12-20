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

        if (!httpContext.Request.RouteValues.TryGetValue("gameId", out var gameIdValue)
            || gameIdValue is not string gameIdString
            || !Guid.TryParse(gameIdString, out var gameId))
        {
            return;
        }

        var developer = await developerRepository.GetByUserIdAsync(userId);
        if (developer == null)
        {
            return;
        }

        var game = await gameRepository.GetByIdAsync(gameId);
        if (game != null && game.DeveloperId == developer.Id)
        {
            context.Succeed(requirement);
        }
    }
}