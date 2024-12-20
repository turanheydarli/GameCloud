using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Repositories;

public interface IActionLogRepository
{
    Task<ActionLog> CreateAsync(ActionLog actionLog);
    Task<ICollection<ActionLog>> GetBySessionAsync(Guid sessionId);
}

public interface INotificationRepository
{
    Task<ICollection<Notification>> GetNotificationsByPlayerAsync(Guid playerId, string status);
}

public interface IPlayerRepository
{
    Task<Player?> GetByPlayerIdAsync(string playerId, AuthProvider provider);
    Task<Player> CreateAsync(Player player);
}

public interface IGameKeyRepository
{
    Task<GameKey> GetByApiKeyAsync(string gameKey);
}