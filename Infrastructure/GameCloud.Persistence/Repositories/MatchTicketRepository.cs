using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class MatchTicketRepository(GameCloudDbContext context) : IMatchTicketRepository
{
    public async Task<IPaginate<MatchTicket>> GetQueueTicketsAsync(Guid queueId, int pageIndex, int pageSize)
    {
        var queue = await context.Set<MatchmakingQueue>()
            .FirstOrDefaultAsync(q => q.Id == queueId);

        var query = context.Set<MatchTicket>()
            .Where(t => t.QueueName == queue.Name);

        var items = query
            .Include(t => t.Player)
            .OrderByDescending(t => t.CreatedAt);

        return await items.ToPaginateAsync(pageIndex, pageSize, 0);
    }

    public async Task<MatchTicket?> GetByIdAsync(Guid ticketId)
    {
        IQueryable<MatchTicket> queryable = context.Set<MatchTicket>();
        return await queryable.FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task CreateAsync(MatchTicket ticket)
    {
        context.Entry(ticket).State = EntityState.Added;
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MatchTicket ticket)
    {
        context.Entry(ticket).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MatchTicket ticket)
    {
        context.Entry(ticket).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }

    public async Task<List<MatchTicket>> GetActiveTicketsAsync(Guid queueId)
    {
        var queue = await context.Set<MatchmakingQueue>().Include(q => q.MatchmakerFunction)
            .FirstOrDefaultAsync(q => q.Id == queueId);

        if (queue == null)
            return new List<MatchTicket>();

        return await context.Set<MatchTicket>()
            .Where(t =>
                t.QueueName == queue.Name &&
                t.Status == TicketStatus.Queued &&
                t.ExpiresAt > DateTime.UtcNow)
            .OrderBy(t => t.CreatedAt).Include(p => p.Player)
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(List<MatchTicket> group)
    {
        context.Set<MatchTicket>().UpdateRange(group);
        await context.SaveChangesAsync();
    }

    public async Task<List<MatchTicket>> GetMatchTicketsAsync(Guid matchId)
    {
        return await context.Set<MatchTicket>()
            .Where(t => t.MatchId == matchId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MatchTicket>> GetPlayerActiveTicketsAsync(Guid playerId)
    {
        return await context.Set<MatchTicket>()
            .Where(t =>
                t.PlayerId == playerId &&
                (t.Status == TicketStatus.Queued ||
                 t.Status == TicketStatus.Matching ||
                 t.Status == TicketStatus.MatchFound) &&
                t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}