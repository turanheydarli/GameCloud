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
                throw new UnauthorizedAccessException("Game Key header is missing.");
            }

            var gameKey = gameKeyValue.ToString();

            Guid gameId;

            try
            {
                gameId = await gameKeyResolver.ResolveGameIdAsync(gameKey);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Failed to validate game key: {ex.Message}");
            }

            if (gameId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Invalid or missing game key.");
            }

            var resolvedGameContext = new GameContext(gameId);
            context.HttpContext.Items["GameContext"] = resolvedGameContext;

            await next();
        }
    }
}