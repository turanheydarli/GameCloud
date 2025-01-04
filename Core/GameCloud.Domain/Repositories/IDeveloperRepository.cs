using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface IDeveloperRepository
{
    Task<IPaginate<Developer>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true);
    Task<Developer> CreateAsync(Developer developer);
    Task<Developer?> GetByIdAsync(Guid id);
    Task<Developer?> GetByUserIdAsync(Guid userId);
    Task<Developer> UpdateAsync(Developer developer);
}