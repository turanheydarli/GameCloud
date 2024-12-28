using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Repositories;

public interface IActionLogRepository
{
    Task<ActionLog> CreateAsync(ActionLog actionLog);
    Task<IPaginate<ActionLog>> GetBySessionAsync(Guid sessionId, int index=0, int size=10);
}

public interface IGameKeyRepository
{
    Task<GameKey> GetByApiKeyAsync(string gameKey);
}