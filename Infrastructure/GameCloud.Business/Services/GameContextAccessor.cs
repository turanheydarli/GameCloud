using GameCloud.Application.Features.Games;
using Microsoft.AspNetCore.Http;

namespace GameCloud.Business.Services;

public class GameContextAccessor(IHttpContextAccessor httpContextAccessor) : IGameContext
{
    public Guid GameId
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context?.Items["GameContext"] is GameContext gc)
            {
                return gc.GameId;
            }

            throw new InvalidOperationException("GameContext is not set.");
        }
    }
}