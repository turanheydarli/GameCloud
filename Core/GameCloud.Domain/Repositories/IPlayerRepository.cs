using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Repositories;

public interface IPlayerRepository
{
    Task<IPaginate<Player>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true);
    Task<Player> CreateAsync(Player player);
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player?> GetByUserIdAsync(Guid userId);
    Task<Player?> GetByUsernameAsync(string playerId, AuthProvider provider);
    Task<Player?> GetByUsernameAsync(string playerId);
    Task<Player> UpdateAsync(Player player);
    Task<IPaginate<Player>> GetAllByGameId(Guid gameId, int index, int size, bool enableTracking = true);
    
    Task<Player> GetByDeviceIdAsync(Guid gameId, string deviceId);
    Task<Player> GetByCustomIdAsync(Guid gameId, string customId);
}