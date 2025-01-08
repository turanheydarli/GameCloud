using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GameCloud.Persistence.Repositories;

public class DeveloperRepository(GameCloudDbContext context) : IDeveloperRepository
{
    public async Task<IPaginate<Developer>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true)
    {
        IQueryable<Developer> queryable = context.Set<Developer>();
        if (!enableTracking)
            queryable.AsNoTracking();

        return await queryable.ToPaginateAsync(index, size, 0);
    }

    public async Task<Developer> CreateAsync(Developer developer)
    {
        context.Entry(developer).State = EntityState.Added;
        await context.SaveChangesAsync();
        return developer;
    }

    public async Task<Developer?> GetByIdAsync(Guid id,
        Func<IQueryable<Developer>, IIncludableQueryable<Developer, object>> include = null)
    {
        IQueryable<Developer?> queryable = context.Set<Developer>();

        queryable = queryable.Where(developer => developer != null && developer.Id == id);
        if (include != null) queryable = include(queryable);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Developer?> GetByUserIdAsync(Guid userId)
    {
        IQueryable<Developer?> queryable = context.Set<Developer>();

        queryable = queryable.Where(developer => developer != null && developer.UserId == userId);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Developer> UpdateAsync(Developer developer)
    {
        context.Entry(developer).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return developer;
    }
}