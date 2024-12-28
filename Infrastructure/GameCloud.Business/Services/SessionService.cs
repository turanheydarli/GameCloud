using AutoMapper;
using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Sessions;
using GameCloud.Application.Features.Sessions.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class SessionService(
    IGameContext gameContext,
    ISessionRepository sessionRepository,
    IMapper mapper) : ISessionService
{
    public async Task<SessionResponse> CreateSessionAsync(Guid userId, Guid gameId)
    {
        var session = new Session
        {
            Status = SessionStatus.Active,
            GameId = gameId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        session = await sessionRepository.CreateAsync(session);

        return mapper.Map<SessionResponse>(session);
    }

    public Task JoinSessionAsync(Guid sessionId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<GameState> GetSessionStateAsync(string sessionId)
    {
        throw new NotImplementedException();
    }
}