using System.Linq.Expressions;
using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GameCloud.Domain.Repositories;

public interface IGameRepository
{
    public static readonly Func<IQueryable<Game>, IIncludableQueryable<Game, object>> DefaultIncludes =
        query => query
            .Include(g => g.Image)
            .Include(g => g.Developer)!;

    public static readonly Func<IQueryable<Game>, IIncludableQueryable<Game, object>> FullGameIncludes =
        query => query
            .Include(g => g.Developer)
            .Include(g => g.GameKeys)
            .Include(g => g.Image).ThenInclude(g => g.Variants);

    Task<IPaginate<Game>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true);

    Task<IPaginate<Game>> GetAllByDeveloperIdAsync(Guid developerId, int index = 0, int size = 10,
        Func<IQueryable<Game>, IIncludableQueryable<Game, object>>? include = null);

    Task<IPaginate<GameKey>> GetAllKeysAsync(Guid gameId, int index = 0, int size = 10);
    Task<Game> CreateAsync(Game developer);
    Task<Game?> GetByIdAsync(Guid id, Func<IQueryable<Game>, IIncludableQueryable<Game, object>>? include = null);
    Task<Game> UpdateAsync(Game game);
    Task DeleteAsync(Game game);
}