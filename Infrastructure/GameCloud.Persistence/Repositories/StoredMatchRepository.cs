using System.Linq.Expressions;
using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class StoredMatchRepository(GameCloudDbContext context) : IStoredMatchRepository
{
    public async Task<StoredMatch?> GetByIdAsync(Guid id)
    {
        return await context.StoredMatches
            .Include(x => x.Players)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<StoredMatch>> GetByGameAndQueueAsync(Guid gameId, string queueName,
        Expression<Func<StoredMatch, bool>>? filter = null, int limit = 100)
    {
        var query = context.StoredMatches
            .Include(x => x.Players)
            .Where(x => x.GameId == gameId &&
                        x.QueueName == queueName &&
                        x.IsAvailableForMatching);

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query
            .OrderByDescending(x => x.CompletedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async  Task<StoredMatch> CreateAsync(StoredMatch match)
    {
        context.Entry(match).State = EntityState.Added;
        await context.SaveChangesAsync();

        return match;
    }

    public async Task UpdateAsync(StoredMatch match)
    {
        context.Entry(match).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var match = await GetByIdAsync(id);
        if (match != null)
        {
            context.Entry(match).State = EntityState.Deleted;
            await context.SaveChangesAsync();
        }
    }
}


public class StoredPlayerRepository(GameCloudDbContext context) : IStoredPlayerRepository
{
    public async Task<StoredPlayer?> GetByIdAsync(Guid id)
    {
        return await context.StoredPlayers
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<StoredPlayer?> CreateAsync(StoredPlayer player)
    {
        context.Entry(player).State = EntityState.Added;
        await context.SaveChangesAsync();

        return player;
    }

    public async Task UpdateAsync(StoredPlayer player)
    {
        context.Entry(player).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var player = await GetByIdAsync(id);
        if (player != null)
        {
            context.Entry(player).State = EntityState.Deleted;
            await context.SaveChangesAsync();
        }
    }
}