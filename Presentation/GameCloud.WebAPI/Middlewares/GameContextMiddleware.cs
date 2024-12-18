using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Games;

namespace GameCloud.WebAPI.Middlewares;

public class GameContextMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext, IGameKeyResolver gameKeyResolver)
    {
        if (!httpContext.Request.Headers.TryGetValue("X-Game-Key", out var gameKeyValue))
        {
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsync("Game key is missing.");
            return;
        }

        var gameKey = gameKeyValue.ToString();

        try
        {
            var gameId = await gameKeyResolver.ResolveGameIdAsync(gameKey);

            var gameContext = new GameContext(gameId);

            httpContext.Items["GameContext"] = gameContext;

            await next(httpContext);
        }
        catch (UnauthorizedAccessException)
        {
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsync("Invalid game key.");
        }
    }
}