using GameCloud.Domain.Dynamics;
using GameCloud.Domain.Entities.Leaderboards;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class LeaderboardRepository(GameCloudDbContext context) : ILeaderboardRepository
{
    public async Task<Leaderboard> CreateAsync(Leaderboard leaderboard)
    {
        context.Entry(leaderboard).State = EntityState.Added;
        await context.SaveChangesAsync();
        return leaderboard;
    }

    public async Task<Leaderboard?> GetByIdAsync(Guid id)
    {
        return await context.Set<Leaderboard>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Leaderboard?> GetSingleAsync(Guid? leaderboardId, Guid? gameId, string? leaderboardName)
    {
        IQueryable<Leaderboard> query = context.Set<Leaderboard>();

        if (leaderboardId.HasValue)
            query = query.Where(x => x.Id == leaderboardId.Value);

        if (gameId.HasValue)
            query = query.Where(x => x.GameId == gameId.Value);

        if (!string.IsNullOrEmpty(leaderboardName))
            query = query.Where(x => x.Name == leaderboardName);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Leaderboard> UpdateAsync(Leaderboard leaderboard)
    {
        context.Entry(leaderboard).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return leaderboard;
    }

    public async Task DeleteAsync(Leaderboard leaderboard)
    {
        context.Entry(leaderboard).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }

    public async Task<IPaginate<Leaderboard>> GetPagedDynamicLeaderboards(DynamicRequest request)
    {
        IQueryable<Leaderboard> query = context.Set<Leaderboard>();
        query = query.ToDynamic(request);
        return await query.ToPaginateAsync(request.PageIndex, request.PageSize, 0);
    }

    public async Task<LeaderboardRecord> CreateRecordAsync(LeaderboardRecord record)
    {
        context.Entry(record).State = EntityState.Added;
        await context.SaveChangesAsync();
        return record;
    }

    public async Task<LeaderboardRecord> UpdateRecordAsync(LeaderboardRecord record)
    {
        context.Entry(record).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return record;
    }

    public async Task<List<LeaderboardRecord>> GetLeaderboardRecordsAsync(Guid leaderboardId, int? limit = 100,
        int? offset = 0)
    {
        return await context.Set<LeaderboardRecord>()
            .Where(r => r.LeaderboardId == leaderboardId)
            .OrderByDescending(r => r.Score)
            .Skip(offset ?? 0)
            .Take(limit ?? 100)
            .ToListAsync();
    }

    public async Task<LeaderboardRecord?> GetUserLeaderboardRecordAsync(Guid leaderboardId, Guid playerId)
    {
        return await context.Set<LeaderboardRecord>()
            .FirstOrDefaultAsync(r => r.LeaderboardId == leaderboardId && r.PlayerId == playerId);
    }

    public async Task<List<LeaderboardRecord>> GetUserLeaderboardRecordsAsync(Guid playerId, int? limit = 100)
    {
        return await context.Set<LeaderboardRecord>()
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.Score)
            .Take(limit ?? 100)
            .ToListAsync();
    }

    public async Task DeleteLeaderboardRecordsAsync(Guid leaderboardId)
    {
        var records = await context.Set<LeaderboardRecord>()
            .Where(r => r.LeaderboardId == leaderboardId)
            .ToListAsync();

        context.Set<LeaderboardRecord>().RemoveRange(records);
        await context.SaveChangesAsync();
    }
}