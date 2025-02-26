using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class MatchmakingQueueRepository(GameCloudDbContext context) : IMatchmakingQueueRepository
{
    public async Task<MatchmakingQueue?> GetByIdWithFunctionsAsync(Guid queueId)
    {
        return await context.Set<MatchmakingQueue>()
            .Include(q => q.InitializeFunction)
            .Include(q => q.TransitionFunction)
            .Include(q => q.LeaveFunction)
            .Include(q => q.EndFunction)
            .FirstOrDefaultAsync(q => q.Id == queueId);
    }

    public async Task<List<MatchmakingQueue>> GetByIdsAsync(List<Guid> queueIds)
    {
        return await context.Set<MatchmakingQueue>()
            .Where(q => queueIds.Contains(q.Id))
            .Include(q => q.MatchmakerFunction)
            .ToListAsync();
    }

    public async Task<IPaginate<MatchmakingQueue>> GetPagedAsync(Guid gameId, string? search, int pageIndex,
        int pageSize)
    {
        var query = context.Set<MatchmakingQueue>()
            .Where(q => q.GameId == gameId);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(q =>
                q.Name.Contains(search) ||
                q.Description.Contains(search));
        }

        var items = query
            .Include(q => q.MatchmakerFunction)
            .OrderByDescending(q => q.CreatedAt);

        return await items.ToPaginateAsync(pageIndex, pageSize);
    }

    public async Task<List<MatchmakingQueue>> GetByGameIdAsync(Guid gameId)
    {
        return await context.Set<MatchmakingQueue>()
            .Where(q => q.GameId == gameId)
            .Include(q => q.MatchmakerFunction)
            .ToListAsync();
    }

    public async Task<MatchmakingQueue?> GetByGameAndNameAsync(Guid gameId, string queueName)
    {
        IQueryable<MatchmakingQueue> queryable = context.Set<MatchmakingQueue>();

        return await queryable
            .FirstOrDefaultAsync(q =>
                q.GameId == gameId &&
                q.Name == queueName);
    }

    public async Task<MatchmakingQueue?> GetByIdAsync(Guid queueId)
    {
        IQueryable<MatchmakingQueue> queryable = context.Set<MatchmakingQueue>();
        queryable = queryable.Include(q => q.MatchmakerFunction);
        return await queryable.FirstOrDefaultAsync(q => q.Id == queueId);
    }

    public async Task<IEnumerable<MatchmakingQueue>> GetAllAsync()
    {
        IQueryable<MatchmakingQueue> queryable = context.Set<MatchmakingQueue>();
        queryable.Include(q => q.MatchmakerFunction);
        return await queryable.ToListAsync();
    }

    public async Task CreateAsync(MatchmakingQueue queue)
    {
        context.Entry(queue).State = EntityState.Added;
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MatchmakingQueue queue)
    {
        context.Entry(queue).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MatchmakingQueue queue)
    {
        context.Entry(queue).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }
}