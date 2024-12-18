using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Sessions;

namespace GameCloud.Business.Services;

public class SessionService : ISessionService
{
    private readonly IGameContext _gameContext;
    private readonly ISessionCache _sessionCache;

    public Task<Guid> CreateSessionAsync(Guid hostUserId)
    {
        var gameId = _gameContext.GameId;

        throw new NotImplementedException();
    }

    public Task JoinSessionAsync(Guid sessionId, Guid userId)
    {
        throw new NotImplementedException();
    }
}