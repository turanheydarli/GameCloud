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

    public async Task<List<ActionLog>> GetListActionByFunctionAsync(Guid functionId, DateTime rangeFrom,
        DateTime rangeTo)
    {
        IQueryable<ActionLog> queryable = context.Set<ActionLog>();

        queryable = queryable.Include(e => e.Function);

        queryable = queryable.Where(k =>
            k.FunctionId == functionId && k.CreatedAt >= rangeFrom && k.CompletedAt <= rangeTo);

        return await queryable.ToListAsync();
    }

    public async Task<IPaginate<ActionLog>> GetByFunctionAsync(Guid functionId, int index = 0, int size = 10)
    {
        IQueryable<ActionLog> queryable = context.Set<ActionLog>();

        queryable = queryable.Where(k => k.FunctionId == functionId);

        return await queryable.ToPaginateAsync(index, size, 0);
    }

    public async Task<IPaginate<ActionLog>> GetTestedActionsByFunctionAsync(Guid functionId, int index = 0,
        int size = 10)
    {
        IQueryable<ActionLog> queryable = context.Set<ActionLog>();

        queryable = queryable
            .Where(k => k.FunctionId == functionId && k.IsTestMode)
            .OrderByDescending(l => l.StartedAt);

        return await queryable.ToPaginateAsync(index, size, 0);
    }

    public async Task<ActionLog?> GetByIdAsync(Guid actionId)
    {
        IQueryable<ActionLog> queryable = context.Set<ActionLog>();

        queryable = queryable.Where(k => k.Id == actionId);

        return await queryable.FirstOrDefaultAsync();
    }
}