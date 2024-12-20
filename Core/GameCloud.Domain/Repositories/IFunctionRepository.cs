using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface IFunctionRepository
{
    Task<FunctionConfig> CreateAsync(FunctionConfig functionConfig);
    Task<FunctionConfig> GetByIdAsync(Guid id);
    Task<FunctionConfig> GetByActionTypeAsync(string actionType);
}