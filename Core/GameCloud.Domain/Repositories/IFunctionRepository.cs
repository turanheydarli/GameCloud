using System.Linq.Expressions;
using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface IFunctionRepository
{
    Task<FunctionConfig> CreateAsync(FunctionConfig functionConfig);
    Task<FunctionConfig> GetByActionTypeAsync(string actionType);
    Task<FunctionConfig?> GetAsync(Expression<Func<FunctionConfig, bool>>? predicate = null);

    Task<IPaginate<FunctionConfig>> GetListPagedByGameIdAsync(
        Guid gameId,
        string? search = null,
        bool ascending = true,
        int page = 0,
        int size = 10,
        bool enableTracking = true);

    Task<FunctionConfig> UpdateAsync(FunctionConfig function);
    Task DeleteAsync(FunctionConfig function);
    Task<List<FunctionConfig>> GetListAsync(Guid gameId, bool enableTracking = true);
}