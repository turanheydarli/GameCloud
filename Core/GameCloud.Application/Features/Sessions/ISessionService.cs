using GameCloud.Application.Features.Sessions.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Sessions;

public interface ISessionService
{
    Task<SessionResponse> CreateSessionAsync(Guid userId, Guid gameId);  
    Task JoinSessionAsync(Guid sessionId, Guid userId);
    Task<GameState> GetSessionStateAsync(string sessionId);
}