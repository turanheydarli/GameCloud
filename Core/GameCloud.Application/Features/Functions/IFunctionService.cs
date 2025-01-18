using GameCloud.Domain.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Features.Functions;

public interface IFunctionService
{
    Task<FunctionResponse> CreateFunctionAsync(Guid gameId, FunctionRequest request, Guid userId);
    Task<PageableListResponse<FunctionResponse>> GetFunctionsAsync(Guid gameId, PageableRequest request);
    Task<FunctionResponse> UpdateAsync(Guid functionId, FunctionRequest request);
    Task<FunctionResponse> ToggleFunction(Guid gameId, Guid functionId, bool isEnabled);
    Task<FunctionResponse> GetById(Guid gameId, Guid functionId);
    Task DeleteAsync(Guid functionId);
}