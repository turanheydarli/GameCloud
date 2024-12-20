using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface IGameRepository
{
    Task<IPaginate<Game>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true);
    Task<IPaginate<GameKey>> GetAllKeysAsync(Guid gameId, int index = 0, int size = 10);
    Task<Game> CreateAsync(Game developer);
    Task<Game?> GetByIdAsync(Guid id);
    Task<Game> UpdateAsync(Game game);
}