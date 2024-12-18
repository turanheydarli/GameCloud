using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Games;
using GameCloud.WebAPI.Filters.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace GameCloud.WebAPI.Filters
{
    public class RequireGameKeyFilter(IGameKeyResolver gameKeyResolver) : IAsyncResourceFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var endpoint = context.ActionDescriptor.EndpointMetadata;
            bool requiresGameKey = endpoint.Any(meta => meta is RequireGameKeyAttribute);

            if (!requiresGameKey)
            {
                await next();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Game-Key", out var gameKeyValue))
            {
                context.Result = new UnauthorizedObjectResult("Game key is missing.");
                return;
            }

            var gameKey = gameKeyValue.ToString();

            Guid gameId;

            try
            {
                gameId = await gameKeyResolver.ResolveGameIdAsync(gameKey);
            }
            catch (Exception ex)
            {
                context.Result = new UnauthorizedObjectResult($"Failed to validate game key: {ex.Message}");
                return;
            }

            if (gameId == Guid.Empty)
            {
                context.Result = new UnauthorizedObjectResult("Invalid or missing game key.");
                return;
            }

            var resolvedGameContext = new GameContext(gameId);
            context.HttpContext.Items["GameContext"] = resolvedGameContext;

            await next();
        }
    }
}