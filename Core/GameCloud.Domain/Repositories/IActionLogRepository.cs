using GameCloud.Domain.Dynamics;
using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface IActionLogRepository
{
    Task<ActionLog> CreateAsync(ActionLog actionLog);
    Task<IPaginate<ActionLog>> GetBySessionAsync(Guid sessionId, int index = 0, int size = 10);
    Task<List<ActionLog>> GetListActionByFunctionAsync(Guid functionId, DateTime rangeFrom, DateTime rangeTo);
    Task<IPaginate<ActionLog>> GetByFunctionAsync(Guid functionId, int index = 0, int size = 10);
    Task<IPaginate<ActionLog>> GetPagedDynamicFunctionLogs(Guid functionId, DynamicRequest request);
    Task<ActionLog?> GetByIdAsync(Guid actionId);
}

public interface IGameKeyRepository
{
    Task<GameKey> CreateAsync(GameKey gameKey);
    Task<GameKey?> GetDefaultByGameId(Guid gameId);
    Task<GameKey> GetByApiKeyAsync(string gameKey);
    Task RevokeAsync(GameKey gameKey);
}