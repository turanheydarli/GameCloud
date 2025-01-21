using GameCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace GameCloud.Domain.Repositories;

public interface IDeveloperRepository
{
    Task<IPaginate<Developer>> GetAllAsync(
        string? search = null,
        bool ascending = true,
        int page = 0,
        int size = 10, bool enableTracking = true);

    Task<Developer> CreateAsync(Developer developer);

    Task<Developer?> GetByIdAsync(Guid id,
        Func<IQueryable<Developer>, IIncludableQueryable<Developer, object>> include = null);

    Task<Developer?> GetByUserIdAsync(Guid userId);
    Task<Developer> UpdateAsync(Developer developer);
}