using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GameCloud.Persistence.Repositories;

public class GameRepository(GameCloudDbContext context) : IGameRepository
{
    public Task<IPaginate<Game>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true)
    {
        throw new NotImplementedException();
    }

    public async Task<IPaginate<Game>> GetAllByDeveloperIdAsync(
        Guid developerId,
        string? search = null,
        bool ascending = true,
        int page = 0,
        int size = 10,
        bool enableTracking = true,
        Func<IQueryable<Game>, IIncludableQueryable<Game, object>>? include = null)
    {
        IQueryable<Game> queryable = context.Set<Game>();

        queryable = queryable.Where(k => k.DeveloperId == developerId);
        if (include != null)
            queryable = include(queryable!);

        if (!enableTracking)
            queryable = queryable.AsNoTracking();

        queryable = ascending
            ? queryable.OrderBy(f => f.CreatedAt)
            : queryable.OrderByDescending(f => f.CreatedAt);

        if (!string.IsNullOrEmpty(search))
        {
            queryable = queryable.Where(f =>
                f.Name.Contains(search) ||
                f.Description.Contains(search));
        }

        return await queryable.ToPaginateAsync(page, size, 0);
    }

    public async Task<IPaginate<GameKey>> GetAllKeysAsync(Guid gameId, int index = 0, int size = 10)
    {
        IQueryable<GameKey> queryable = context.Set<GameKey>();

        queryable = queryable.Where(k => k.GameId == gameId);

        return await queryable.ToPaginateAsync(index, size, 0);
    }

    public async Task<Game> CreateAsync(Game game)
    {
        context.Entry(game).State = EntityState.Added;
        await context.SaveChangesAsync();
        return game;
    }

    public async Task<Game?> GetByIdAsync(Guid id,
        Func<IQueryable<Game>, IIncludableQueryable<Game, object>>? include = null)
    {
        IQueryable<Game?> queryable = context.Set<Game>();

        queryable = queryable.Where(game => game != null && game.Id == id);

        if (include != null)
            queryable = include(queryable!);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Game> UpdateAsync(Game game)
    {
        context.Entry(game).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return game;
    }

    public async Task DeleteAsync(Game game)
    {
        context.Entry(game).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }
}