using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface IActionLogRepository
{
    Task<ActionLog> CreateAsync(ActionLog actionLog);
    Task<IPaginate<ActionLog>> GetBySessionAsync(Guid sessionId, int index = 0, int size = 10);
    Task<List<ActionLog>> GetListActionByFunctionAsync(Guid functionId, DateTime rangeFrom, DateTime rangeTo);
    Task<IPaginate<ActionLog>> GetByFunctionAsync(Guid functionId, int index = 0, int size = 10);
    Task<IPaginate<ActionLog>> GetTestedActionsByFunctionAsync(Guid functionId, int index = 0, int size = 10);
    Task<ActionLog?> GetByIdAsync(Guid actionId);
}

public interface IGameKeyRepository
{
    Task<GameKey> GetByApiKeyAsync(string gameKey);
    Task RevokeAsync(GameKey gameKey);
}