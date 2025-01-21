using System.Linq.Expressions;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class FunctionRepository(GameCloudDbContext context) : IFunctionRepository
{
    public async Task<FunctionConfig> CreateAsync(FunctionConfig functionConfig)
    {
        context.Entry(functionConfig).State = EntityState.Added;
        await context.SaveChangesAsync();
        return functionConfig;
    }

    public async Task<FunctionConfig> GetByActionTypeAsync(Guid gameId, string actionType)
    {
        IQueryable<FunctionConfig?> queryable = context.Set<FunctionConfig>();

        queryable = queryable.Where(f => f.ActionType == actionType && f.GameId == gameId);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<FunctionConfig?> GetAsync(Expression<Func<FunctionConfig, bool>>? predicate = null)
    {
        IQueryable<FunctionConfig?> queryable = context.Set<FunctionConfig>();

        if (predicate is not null)
        {
            queryable = queryable.Where(predicate!);
        }

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<IPaginate<FunctionConfig>> GetListPagedByGameIdAsync(
        Guid gameId,
        string? search = null,
        bool ascending = true,
        int page = 0,
        int size = 10,
        bool enableTracking = true)
    {
        IQueryable<FunctionConfig> queryable = context.Set<FunctionConfig>();
        if (!enableTracking)
            queryable.AsNoTracking();

        queryable = queryable.Where(f => f.GameId == gameId);

        if (!string.IsNullOrEmpty(search))
            queryable = queryable.Where(f =>
                f.Name.Contains(search) ||
                f.Description.Contains(search) ||
                f.ActionType.Contains(search));

        return await queryable.ToPaginateAsync(page, size, 0);
    }

    public async Task<FunctionConfig> UpdateAsync(FunctionConfig functionConfig)
    {
        context.Entry(functionConfig).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return functionConfig;
    }

    public async Task DeleteAsync(FunctionConfig function)
    {
        context.Entry(function).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }

    public async Task<List<FunctionConfig>> GetListAsync(Guid gameId, bool enableTracking = false)
    {
        IQueryable<FunctionConfig> queryable = context.Set<FunctionConfig>();
        if (!enableTracking)
            queryable.AsNoTracking();

        queryable = queryable.Where(f => f.GameId == gameId);

        return await queryable.ToListAsync();
    }
}