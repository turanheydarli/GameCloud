using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class GameRepository(GameCloudDbContext context) : IGameRepository
{
    public Task<IPaginate<Game>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true)
    {
        throw new NotImplementedException();
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

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        IQueryable<Game?> queryable = context.Set<Game>();

        queryable = queryable.Where(game => game != null && game.Id == id);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Game> UpdateAsync(Game game)
    {
        context.Entry(game).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return game;
    }
}