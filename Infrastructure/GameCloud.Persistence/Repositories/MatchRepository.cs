using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class MatchRepository(GameCloudDbContext context) : IMatchRepository
{
    public async Task<Match?> GetByIdAsync(Guid matchId)
    {
        IQueryable<Match> queryable = context.Set<Match>();
        return await queryable.FirstOrDefaultAsync(m => m.Id == matchId);
    }

    public async Task CreateAsync(Match match)
    {
        context.Entry(match).State = EntityState.Added;
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Match match)
    {
        context.Entry(match).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task<List<Match>> GetTimeoutMatchesAsync(DateTime utcNow)
    {
        return await context.Set<Match>()
            .Where(m =>
                // Match is in progress or waiting for turn
                (m.State == MatchState.InProgress || m.State == MatchState.WaitingTurn) &&
                // Has a next action deadline
                m.NextActionDeadline.HasValue &&
                // Deadline has passed
                m.NextActionDeadline.Value < utcNow &&
                // Not already handled
                m.State != MatchState.Completed &&
                m.State != MatchState.Cancelled &&
                m.State != MatchState.Error
            )
            .ToListAsync();
    }

    public async Task<List<Match>> GetPlayerActiveMatchesAsync(Guid playerId)
    {
        return await context.Set<Match>()
            .Where(m =>
                m.PlayerIds.Contains(playerId) &&
                (m.State == MatchState.Created ||
                 m.State == MatchState.Ready ||
                 m.State == MatchState.InProgress ||
                 m.State == MatchState.WaitingTurn) &&
                (!m.MatchTimeout.HasValue ||
                 m.CreatedAt.AddTicks(m.MatchTimeout.Value.Ticks) > DateTime.UtcNow)
            )
            .OrderByDescending(m => m.LastActionAt ?? m.CreatedAt)
            .ToListAsync();
    }
}

public class MatchTicketRepository(GameCloudDbContext context) : IMatchTicketRepository
{
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
            .OrderBy(t => t.CreatedAt)
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

public class MatchmakingQueueRepository(GameCloudDbContext context) : IMatchmakingQueueRepository
{
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