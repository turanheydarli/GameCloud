using System.Linq.Expressions;
using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface IFunctionRepository
{
    Task<FunctionConfig> CreateAsync(FunctionConfig functionConfig);
    Task<FunctionConfig> GetByActionTypeAsync(string actionType);
    Task<FunctionConfig?> GetAsync(Expression<Func<FunctionConfig, bool>>? predicate = null);

    Task<IPaginate<FunctionConfig>> GetListAsync(Expression<Func<FunctionConfig, bool>>? predicate = null,
        int index = 0, int size = 10, bool enableTracking = true);

    Task<FunctionConfig> UpdateAsync(FunctionConfig function);
    Task DeleteAsync(FunctionConfig function);
}