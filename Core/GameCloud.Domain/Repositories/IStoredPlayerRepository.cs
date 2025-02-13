using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Domain.Repositories;

public interface IStoredPlayerRepository
{
    Task<StoredPlayer?> GetByIdAsync(Guid id);
    Task<StoredPlayer?> CreateAsync(StoredPlayer player);
    Task UpdateAsync(StoredPlayer player);
    Task DeleteAsync(Guid id);
}