using GameCloud.Application.Features.Sessions.Models;

namespace GameCloud.Application.Common.Interfaces;

public interface ISessionCache
{
    Task<SessionInfo> GetSessionAsync(string sessionId);
    Task SetSessionAsync(string sessionId, SessionInfo session);
    Task RemoveSessionAsync(string sessionId);
    Task<Dictionary<string, Dictionary<string, object>>> GetSessionStateAsync(Guid sessionId);
    Task UpdateSessionStateAsync(Guid sessionId, Dictionary<string, Dictionary<string, object>> changes);
}