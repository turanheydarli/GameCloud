using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Repositories;

public interface IPlayerRepository
{
    Task<IPaginate<Player>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true);
    Task<Player> CreateAsync(Player player);
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player?> GetByUserIdAsync(Guid userId);
    Task<Player?> GetByPlayerIdAsync(string playerId, AuthProvider provider);
    Task<Player?> GetByPlayerIdAsync(string playerId);
    Task<Player> UpdateAsync(Player player);
}