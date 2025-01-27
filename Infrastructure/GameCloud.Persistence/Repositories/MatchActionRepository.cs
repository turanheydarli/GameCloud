using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class MatchActionRepository(GameCloudDbContext context) : IMatchActionRepository
{
    public async Task<MatchAction> CreateAsync(MatchAction action)
    {
        context.Set<MatchAction>().Add(action);
        await context.SaveChangesAsync();
        return action;
    }

    public async Task<MatchAction?> GetByIdAsync(Guid id)
    {
        return await context.Set<MatchAction>()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<MatchAction> UpdateAsync(MatchAction action)
    {
        context.Set<MatchAction>().Update(action);
        await context.SaveChangesAsync();
        return action;
    }

    public async Task DeleteAsync(MatchAction action)
    {
        context.Set<MatchAction>().Remove(action);
        await context.SaveChangesAsync();
    }

    public async Task<List<MatchAction>> GetMatchActionsAsync(Guid matchId, DateTime? since = null, int? limit = null)
    {
        var query = context.Set<MatchAction>()
            .Where(a => a.MatchId == matchId);

        if (since.HasValue)
            query = query.Where(a => a.Timestamp > since.Value);

        query = query.OrderByDescending(a => a.Timestamp);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync();
    }

    public async Task<List<MatchAction>> GetPlayerActionsAsync(Guid playerId, DateTime? since = null, int? limit = null)
    {
        var query = context.Set<MatchAction>()
            .Where(a => a.PlayerId == playerId);

        if (since.HasValue)
            query = query.Where(a => a.Timestamp > since.Value);

        query = query.OrderByDescending(a => a.Timestamp);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync();
    }

    public async Task<MatchAction?> GetLatestActionAsync(Guid matchId)
    {
        return await context.Set<MatchAction>()
            .Where(a => a.MatchId == matchId)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetActionCountAsync(Guid matchId)
    {
        return await context.Set<MatchAction>()
            .CountAsync(a => a.MatchId == matchId);
    }
}