using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
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

    public Task<FunctionConfig> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<FunctionConfig> GetByActionTypeAsync(string actionType)
    {
        throw new NotImplementedException();
    }

    public async Task<FunctionConfig> UpdateAsync(FunctionConfig functionConfig)
    {
        context.Entry(functionConfig).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return functionConfig;
    }
}