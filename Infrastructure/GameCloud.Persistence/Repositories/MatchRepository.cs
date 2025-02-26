using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class MatchRepository(GameCloudDbContext context) : IMatchRepository
{
    public async Task<int> GetActiveMatchesCountAsync(Guid gameId, List<Guid>? queueIds = null)
    {
        var query = context.Set<Match>()
            .Where(m => m.GameId == gameId &&
                        (m.State == MatchStatus.InProgress ||
                         m.State == MatchStatus.Ready));

        if (queueIds != null && queueIds.Any())
        {
            var queueNames = await context.Set<MatchmakingQueue>()
                .Where(q => queueIds.Contains(q.Id))
                .Select(q => q.Name)
                .ToListAsync();

            query = query.Where(m => queueNames.Contains(m.QueueName));
        }

        return await query.CountAsync();
    }

    public async Task<QueueActivityData> GetQueueActivityAsync(Guid queueId, DateTime startDate, DateTime endDate)
    {
        var queue = await context.Set<MatchmakingQueue>()
            .FirstOrDefaultAsync(q => q.Id == queueId);

        if (queue == null)
            return new QueueActivityData();

        var matches = await context.Set<Match>()
            .Where(m =>
                m.QueueName == queue.Name &&
                m.CreatedAt >= startDate &&
                m.CreatedAt <= endDate)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        var timePoints = new List<string>();
        var matchCounts = new List<int>();
        var playerCounts = new List<int>();

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            var dayMatches = matches.Where(m =>
                m.CreatedAt.Date == currentDate.Date).ToList();

            timePoints.Add(currentDate.ToString("yyyy-MM-dd"));
            matchCounts.Add(dayMatches.Count);
            playerCounts.Add(dayMatches.Sum(m => m.PlayerIds.Count));

            currentDate = currentDate.AddDays(1);
        }

        return new QueueActivityData
        {
            TimePoints = timePoints,
            MatchCounts = matchCounts,
            PlayerCounts = playerCounts
        };
    }

    public async Task<IPaginate<Match>> GetQueueMatchesAsync(Guid queueId, string? status, int pageIndex,
        int pageSize)
    {
        var queue = await context.Set<MatchmakingQueue>()
            .FirstOrDefaultAsync(q => q.Id == queueId);

        var query = context.Set<Match>()
            .Where(m => m.QueueName == queue.Name);

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<MatchStatus>(status, true, out var matchStatus))
            {
                query = query.Where(m => m.State == matchStatus);
            }
        }

        var total = await query.CountAsync();
        var items = query
            .OrderByDescending(m => m.CreatedAt);

        return await items.ToPaginateAsync(pageIndex, pageSize);
    }

    public async Task<IPaginate<MatchmakingLogEntry>> GetMatchmakingLogsAsync(
        Guid gameId,
        Guid? queueId,
        string? eventType,
        DateTime startDate,
        DateTime endDate,
        int pageIndex,
        int pageSize)
    {
        var query = context.Set<MatchmakingLogEntry>()
            .Where(l =>
                l.GameId == gameId &&
                l.Timestamp >= startDate &&
                l.Timestamp <= endDate);

        if (queueId.HasValue)
        {
            var queue = await context.Set<MatchmakingQueue>()
                .FirstOrDefaultAsync(q => q.Id == queueId.Value);
            if (queue != null)
            {
                query = query.Where(l => l.QueueName == queue.Name);
            }
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(l => l.EventType == eventType);
        }

        var items = query
            .OrderByDescending(l => l.Timestamp);

        return await items.ToPaginateAsync(pageIndex, pageSize);
    }

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

    public async Task<Match?> GetPlayerActiveMatchAsync(Guid playerId)
    {
        IQueryable<Match?> queryable = context.Set<Match>();

        return await queryable.FirstOrDefaultAsync(m =>
            m.PlayerIds.Contains(playerId) &&
            (m.State == MatchStatus.InProgress || m.State == MatchStatus.Ready));
    }

    public async Task<List<Match>> GetTimeoutMatchesAsync(DateTime utcNow)
    {
        return await context.Set<Match>()
            .Where(m =>
                // Match is in progress or waiting for turn
                (m.State == MatchStatus.InProgress || m.State == MatchStatus.WaitingTurn) &&
                // Has a next action deadline
                m.NextActionDeadline.HasValue &&
                // Deadline has passed
                m.NextActionDeadline.Value < utcNow &&
                // Not already handled
                m.State != MatchStatus.Completed &&
                m.State != MatchStatus.Cancelled &&
                m.State != MatchStatus.Error
            )
            .ToListAsync();
    }

    public async Task<List<Match>> GetPlayerActiveMatchesAsync(Guid playerId)
    {
        return await context.Set<Match>()
            .Where(m =>
                m.PlayerIds.Contains(playerId) &&
                (m.State == MatchStatus.Created ||
                 m.State == MatchStatus.Ready ||
                 m.State == MatchStatus.InProgress ||
                 m.State == MatchStatus.WaitingTurn) &&
                (!m.MatchTimeout.HasValue ||
                 m.CreatedAt.AddTicks(m.MatchTimeout.Value.Ticks) > DateTime.UtcNow)
            )
            .OrderByDescending(m => m.LastActionAt ?? m.CreatedAt)
            .ToListAsync();
    }
}