using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class ActionLogRepository(GameCloudDbContext context) : IActionLogRepository
{
    public async Task<ActionLog> CreateAsync(ActionLog actionLog)
    {
        context.Entry(actionLog).State = EntityState.Added;
        await context.SaveChangesAsync();
        return actionLog;
    }

    public async Task<IPaginate<ActionLog>> GetBySessionAsync(Guid sessionId, int index = 0, int size = 10)
    {
        IQueryable<ActionLog> queryable = context.Set<ActionLog>();

        queryable = queryable.Where(k => k.SessionId == sessionId);

        return await queryable.ToPaginateAsync(index, size, 0);
    }
}