using GameCloud.Application.Features.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GameCloud.Application.Common.Policies.Requirements.Handlers
{
    public class GameKeyRequirementHandler(IGameKeyResolver gameKeyResolver, IHttpContextAccessor httpContextAccessor)
        : AuthorizationHandler<GameKeyRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            GameKeyRequirement requirement)
        {
            var httpContext = httpContextAccessor.HttpContext ?? context.Resource as HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return;
            }

            if (!httpContext.Request.Headers.TryGetValue("X-Game-Key", out var gameKeyValue))
            {
                context.Fail(new AuthorizationFailureReason(this, "Game Key is missing."));
                return;
            }

            var gameKey = gameKeyValue.ToString();

            Guid gameId;
            try
            {
                gameId = await gameKeyResolver.ResolveGameIdAsync(gameKey);
            }
            catch
            {
                context.Fail(new AuthorizationFailureReason(this, "Unable to resolve game key"));
                return;
            }

            if (gameId == Guid.Empty)
            {
                context.Fail();
                return;
            }

            var resolvedGameContext = new GameContext(gameId);
            httpContext.Items["GameContext"] = resolvedGameContext;

            context.Succeed(requirement);
        }
    }
}