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

    public async Task<FunctionConfig> GetByActionTypeAsync(string actionType)
    {
        IQueryable<FunctionConfig?> queryable = context.Set<FunctionConfig>();

        queryable = queryable.Where(f => f.ActionType == actionType);

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

    public async Task<IPaginate<FunctionConfig>> GetListAsync(Expression<Func<FunctionConfig, bool>>? predicate = null,
        int index = 0, int size = 10, bool enableTracking = true)
    {
        IQueryable<FunctionConfig> queryable = context.Set<FunctionConfig>();
        if (!enableTracking)
            queryable.AsNoTracking();

        if (predicate is not null)
            queryable = queryable.Where(predicate);

        return await queryable.ToPaginateAsync(index, size, 0);
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
}