namespace GameCloud.Application.Common.Interfaces;

public interface ISessionCache
{
    Task UpdateSessionStateAsync(Guid sessionId, Dictionary<string, Dictionary<string, object>> changes);
    Task<Dictionary<string, Dictionary<string, object>>> GetSessionStateAsync(Guid sessionId);
}