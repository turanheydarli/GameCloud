namespace GameCloud.Application.Features.Sessions;

public interface ISessionService
{
    Task<Guid> CreateSessionAsync(Guid hostUserId);  
    Task JoinSessionAsync(Guid sessionId, Guid userId);
}